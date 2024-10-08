using MyML.Functions;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyML
{
    public class NN
    {
        public IActFunction ActFunction { get; private set; }
        public int[] LayerSizes { get; private set; }
        public int InputSize { get; private set; }
        public int OutputSize {  get; private set; }
        public Layer[] Layers { get; private set; }
        
        public NN(int[] parameters, IActFunction actFunction, int? randSeed = null)
        {
            // [входных сигналов, нейронов первого слоя, нейронов второго слоя, ...]
            if (parameters.Length < 2)
                throw new ArgumentException();

            ActFunction = actFunction;
            LayerSizes = (int[]) parameters.Clone();
            Layers = new Layer[parameters.Length - 1];

            Random random = (randSeed is null) ? new Random() : new Random(randSeed.Value);

            for (int i = 0; i < Layers.Length; i++)
            {
                Layers[i] = new Layer(parameters[i], parameters[i+1]);
                Layers[i].InitializeWeightMatrix(random);
            }
            InputSize = LayerSizes[0];
            OutputSize = LayerSizes[LayerSizes.Length-1];
        }

        public static Matrix<double> Softmax(Matrix<double> input)
        {
            if (input.ColumnCount != 1)
                throw new ArgumentException();

            double sum = 0;
            var result = input.Clone();
            for (int i = 0; i < input.RowCount; i++)
            {
                result[i, 0] = Math.Exp(input[i, 0]);
                sum += result[i, 0];
            }
            return (1 / sum) * result;
        }

        public Matrix<double> Run(Matrix<double> input)
        {
            if (input.ColumnCount != 1 && input.RowCount != LayerSizes[0])
                throw new ArgumentException("Incorrect input dimensions");

            Matrix<double> result = input.Clone();
            for (int i = 0; i < Layers.Length; i++)
            {
                result = Layers[i].Run(result, ActFunction);
            }
            
            return Softmax(result);
        }

        public void Learn(Matrix<double> output, Matrix<double> targetResult, double learningRate, out double logLoss)
        {
            logLoss = LogLoss(output, targetResult);
            Matrix<double> dLdY = output - targetResult;
            for (int i = Layers.Length - 1; i >= 0; i--)
            {
                dLdY = Layers[i].BackPropAndLearn(dLdY, learningRate, ActFunction, out _);
            }
        }

        public static double LogLoss(Matrix<double> real, Matrix<double> ideal)
        {
            if (real.ColumnCount != 1 || ideal.ColumnCount != 1 || real.RowCount != ideal.RowCount)
                throw new ArgumentException("");

            double result = 0;
            for (int i = 0; i < real.RowCount; i++)
            {
                if (real[i, 0] == 0 || ideal[i, 0] == 0)
                    continue;

                result -= ideal[i, 0] * Math.Log(real[i, 0]);
            }

            return result;
        }

        public Matrix<double> RunAndLearn(Matrix<double> input, Matrix<double> targetResult, double learningRate, out double logLoss)
        {
            Matrix<double> output = Run(input);

            Learn(output, targetResult, learningRate, out logLoss);

            return output;
        }

        public override string ToString()
        {
            string result = $"{Layers.Length}:[ ";
            for (int i = 0; i < LayerSizes.Length; i++)
                result += $"{LayerSizes[i]} ";
            result+= "]: ";
            foreach (var layer in Layers)
                result += layer.ToString();

            return result;
        }
    }
}
