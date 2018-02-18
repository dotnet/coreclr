using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JitBench
{
    public static class Statistics
    {
        public static double SampleStandardDeviation(this IEnumerable<double> data)
        {
            int n = data.Count();
            double sampleMean = data.Average();
            return Math.Sqrt(data.Select(x => (x - sampleMean) * (x - sampleMean)).Sum() / (n - 1));
        }

        public static double StandardError(this IEnumerable<double> data)
        {
            int n = data.Count();
            return SampleStandardDeviation(data) / Math.Sqrt(n);
        }

        public static double MarginOfError95(this IEnumerable<double> data)
        {
            return StandardError(data) * 1.96;
        }
    }
}
