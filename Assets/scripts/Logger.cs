using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;

public class Logger : MonoBehaviour
{
    public List<double> decisionTimes = new List<double>();
    
    public void LogDecisionTime(double decisionTime)
    {
        decisionTimes.Add(decisionTime);
    }
    
    // public void CalculateMetrics()
    // {
    //     if (decisionTimes.Count == 0) return;
    //     double averageTime = decisionTimes.Average();
    //     double maxTime = decisionTimes.Max();
    //     double minTime = decisionTimes.Min();
    //     decisionMetrics.Add(averageTime);
    //     decisionMetrics.Add(maxTime);
    //     decisionMetrics.Add(minTime);
    //     SaveMetricsToCSV("decisionMetrics.csv");
    // }

    public void SaveMetricsToCSV(string path)
    {
        using (StreamWriter writer = new StreamWriter(path, append: true))
        {
            foreach (var time in decisionTimes)
            {
                writer.WriteLine(time);
            }
        }
    }

}
