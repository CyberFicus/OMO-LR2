using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyML
{
    public class Stats
    {
        public int Size { get; private set; }

        double[] Accuracy { get; set; }
        double[] Precision { get; set; }
        double[] Recall { get; set; }
        double Loss { get; set; }

        public Stats(ConfusionMatrix cn, double logLossSum)
        {
            Size = cn.Size;
            double N = cn.N();
            Loss = logLossSum / N;
            Accuracy = new double[Size];
            Precision = new double[Size];
            Recall = new double[Size];

            for (int i = 0; i < Size; i++)
            {
                double TP = cn.TP(i), TN = cn.TN(i), FP = cn.FP(i), FN = cn.FN(i);

                Accuracy[i] = (TP + TN) / N ;
                Precision[i] = TP / (TP + FP);
                Recall[i] = TP / (TP + FN);
            }
        }
    }
}
