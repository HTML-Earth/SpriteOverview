using System;
using System.IO;
using System.Text.RegularExpressions;

namespace SpriteData
{
    internal class Program
    {
        //first argument is path to MERGED folder
        //second argument is the name of a .BAN or .BND folder
        public static void Main(string[] args)
        {
            string rootDir = args[0];
            string banName = args[1];
            string[] filePaths = Directory.GetFiles(rootDir + "\\" + banName);

            int animCount = 0;
            
            //find anim count
            foreach (string filePath in filePaths)
            {
                Match match = Regex.Match(filePath, ".+\\.*_(.+)_(.+)\\.png");
                int animId = int.Parse(match.Groups[1].Value);
                if (animCount < animId)
                    animCount = animId;
            }
            animCount++;
            
            int[] frames = new int[animCount];
            
            //find frame counts
            foreach (string filePath in filePaths)
            {
                Match match = Regex.Match(filePath, ".+\\.*_(.+)_(.+)\\.png");
                
                int id = int.Parse(match.Groups[1].Value);
                int frame = int.Parse(match.Groups[2].Value) + 1;

                if (frames[id] < frame)
                    frames[id] = frame;
            }

            //print out frame counts
            for (int i = 0; i < frames.Length; i++)
            {
                Console.WriteLine($"{frames[i]}");
            }
        }
    }
}