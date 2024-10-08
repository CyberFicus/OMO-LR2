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
