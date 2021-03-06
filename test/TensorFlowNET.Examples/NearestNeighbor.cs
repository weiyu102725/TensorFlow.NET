﻿using NumSharp.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Tensorflow;
using TensorFlowNET.Examples.Utility;

namespace TensorFlowNET.Examples
{
    /// <summary>
    /// A nearest neighbor learning algorithm example
    /// This example is using the MNIST database of handwritten digits
    /// https://github.com/aymericdamien/TensorFlow-Examples/blob/master/examples/2_BasicModels/nearest_neighbor.py
    /// </summary>
    public class NearestNeighbor : Python, IExample
    {
        public int Priority => 5;
        public bool Enabled => true;
        public string Name => "Nearest Neighbor";
        Datasets mnist;
        NDArray Xtr, Ytr, Xte, Yte;

        public bool Run()
        {
            // tf Graph Input
            var xtr = tf.placeholder(tf.float32, new TensorShape(-1, 784));
            var xte = tf.placeholder(tf.float32, new TensorShape(784));

            // Nearest Neighbor calculation using L1 Distance
            // Calculate L1 Distance
            var distance = tf.reduce_sum(tf.abs(tf.add(xtr, tf.negative(xte))), reduction_indices: 1);
            // Prediction: Get min distance index (Nearest neighbor)
            var pred = tf.arg_min(distance, 0);

            float accuracy = 0f;
            // Initialize the variables (i.e. assign their default value)
            var init = tf.global_variables_initializer();
            with(tf.Session(), sess =>
            {
                // Run the initializer
                sess.run(init);

                PrepareData();

                foreach(int i in range(Xte.shape[0]))
                {
                    // Get nearest neighbor
                    long nn_index = sess.run(pred, new FeedItem(xtr, Xtr), new FeedItem(xte, Xte[i]));
                    // Get nearest neighbor class label and compare it to its true label
                    print($"Test {i} Prediction: {np.argmax(Ytr[nn_index])} True Class: {np.argmax(Yte[i] as NDArray)}");
                    // Calculate accuracy
                    if (np.argmax(Ytr[nn_index]) == np.argmax(Yte[i] as NDArray))
                        accuracy += 1f/ Xte.shape[0];
                }

                print($"Accuracy: {accuracy}");
            });

            return accuracy > 0.9;
        }

        public void PrepareData()
        {
            mnist = MnistDataSet.read_data_sets("mnist", one_hot: true);
            // In this example, we limit mnist data
            (Xtr, Ytr) = mnist.train.next_batch(5000); // 5000 for training (nn candidates)
            (Xte, Yte) = mnist.test.next_batch(200); // 200 for testing
        }
    }
}
