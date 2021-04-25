﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpriteOverview
{
    internal class Program
    {
        static List<string> filesToCompare;
        
        public static void Main(string[] args)
        {
            List<string> aeLvls = File.ReadLines("Input_ExoddusLvls.txt").ToList();
            List<string> aoLvls = File.ReadLines("Input_OddyseeLvls.txt").ToList();
            
            List<string> aeBans = File.ReadLines("Input_ExoddusBans.txt").ToList();
            List<string> aoBans = File.ReadLines("Input_OddyseeBans.txt").ToList();
            
            List<string> differingBans = File.ReadLines("Input_DifferingHomonymousBans.txt").ToList();
            
            string cd = Directory.GetCurrentDirectory();
            
            string mergeDir = cd + "\\MERGED";
            
            Dictionary<string, bool> foundBansExoddus = aeBans.ToDictionary(
                aeBan => aeBan,
                aeBan => false);
            
            Dictionary<string, bool> foundBansOddysee = aoBans.ToDictionary(
                aoBan => aoBan,
                aoBan => false);
            
            Dictionary<string, bool> foundSameNameExoddus = new Dictionary<string, bool>();
            Dictionary<string, bool> foundSameNameOddysee = new Dictionary<string, bool>();
            Dictionary<string, bool> foundBansExoddusExclusive = new Dictionary<string, bool>();
            Dictionary<string, bool> foundBansOddyseeExclusive = new Dictionary<string, bool>();
            
            // Fill sameNameBans and bansExoddusExclusive
            foreach (KeyValuePair<string,bool> kv in foundBansExoddus)
            {
                if (foundBansOddysee.ContainsKey(kv.Key))
                    foundSameNameExoddus.Add(kv.Key, false);
                else
                    foundBansExoddusExclusive.Add(kv.Key, false);
            }

            // Fill bansOddyseeExclusive
            foreach (KeyValuePair<string,bool> kv in foundBansOddysee)
            {
                if (foundBansExoddus.ContainsKey(kv.Key))
                    foundSameNameOddysee.Add(kv.Key, false);
                else
                    foundBansOddyseeExclusive.Add(kv.Key, false);
            }
            
            File.WriteAllLines("AutoGenerated_HomonymousBans.txt", foundSameNameExoddus.Keys.ToArray());

            filesToCompare = new List<string>();
            
            //MERGE AE
            MergeFilesInLevelDirectories(cd, aeLvls, foundBansExoddusExclusive, mergeDir);
            MergeFilesInLevelDirectories(cd, aeLvls, foundSameNameExoddus, mergeDir, differingBans, true);
            
            //MERGE AO
            MergeFilesInLevelDirectories(cd, aoLvls, foundBansOddyseeExclusive, mergeDir);
            MergeFilesInLevelDirectories(cd, aoLvls, foundSameNameOddysee, mergeDir, differingBans);
            
            GenerateComparisonPage();
        }

        static void GenerateComparisonPage()
        {
            List<string> lines = new List<string>();
            
            lines.Add("<!DOCTYPE html>");
            lines.Add("<html>");
            lines.Add("\t<head>");
            lines.Add("\t\t<meta charset=\"UTF-8\">");
            lines.Add("\t\t<link rel=\"stylesheet\" type=\"text/css\" href=\"comparison.css\" />");
            lines.Add("\t\t<title>Sprite comparison</title>");
            lines.Add("\t</head>");
            lines.Add("\t<body>");
            lines.Add("\t\t<table>");
            lines.Add("\t\t\t<tr>");
            lines.Add("\t\t\t\t<th>AO</th>");
            lines.Add("\t\t\t\t<th>AE</th>");
            lines.Add("\t\t\t</tr>");

            foreach (string f in filesToCompare)
            {
                FileInfo file = new FileInfo(f);
                
                lines.Add("\t\t\t<tr>");
                lines.Add($"\t\t\t\t<td><img src=\"{f}[AO].gif\"></td>");
                lines.Add($"\t\t\t\t<td><img src=\"{f}[AE].gif\"></td>");
                lines.Add("\t\t\t</tr>");
                lines.Add("\t\t\t<tr>");
                lines.Add($"\t\t\t\t<td class=\"title\" colspan=\"2\">{file.Name}</td>");
                lines.Add("\t\t\t</tr>");
            }

            lines.Add("\t\t</table>");
            lines.Add("\t</body>");
            lines.Add("</html>");
            
            File.WriteAllLines("comparison.html", lines.ToArray());
        }

        static void MergeFilesInLevelDirectories(string rootDir, List<string> levelNames, Dictionary<string, bool> bansToFind, string mergeDir, List<string> differing = null, bool ae = false)
        {
            string[] lvlDirs = Directory.GetDirectories(rootDir);
            
            // Iterate through level directories
            foreach (string lvlDir in lvlDirs)
            {
                DirectoryInfo lvlDirInfo = new DirectoryInfo(lvlDir);
                
                // skip if not in levelNames list
                if (!levelNames.Contains(lvlDirInfo.Name))
                    continue;
                
                string[] lvlDirFiles = Directory.GetFiles(lvlDirInfo.FullName);
                string[] banDirs = Directory.GetDirectories(lvlDirInfo.FullName);
                
                // iterate through ban directories
                foreach (string ban in banDirs)
                {
                    DirectoryInfo banDirInfo = new DirectoryInfo(ban);
                    
                    // skip if ban is not in bansToFind dict
                    if (!bansToFind.ContainsKey(banDirInfo.Name))
                    {
                        //Console.WriteLine($"did not find {banDirInfo.Name} - from {lvlDirInfo.Name}");
                        continue;
                    }
                    
                    // if ban has not been found yet
                    if (!bansToFind[banDirInfo.Name])
                    {
                        //Console.WriteLine($"found {banDirInfo.Name}");
                        bansToFind[banDirInfo.Name] = true;

                        string append = "";

                        bool fileDiffers = false;
                        
                        // optional check if ban is in differing list
                        if (differing != null)
                        {
                            // if ban differs between AE and AO, tag it as such
                            if (differing.Contains(banDirInfo.Name))
                            {
                                append = ae ? "[AE]" : "[AO]";
                                fileDiffers = true;
                            }
                            else
                            {
                                // if ban does not differ, skip the AO version instead of tagging
                                if (!ae)
                                    continue;
                            }
                        }

                        // copy directory / files
                        Console.WriteLine(banDirInfo.Name);
                        DirectoryCopy(banDirInfo.FullName, mergeDir + "\\Frames\\" + banDirInfo.Name + append, true);
                        foreach (string filePath in lvlDirFiles)
                        {
                            string file = Path.GetFileNameWithoutExtension(filePath);
                            if (file.StartsWith(banDirInfo.Name))
                            {
                                if (!Directory.Exists(mergeDir + "\\gifs"))
                                    Directory.CreateDirectory(mergeDir + "\\gifs");
                                
                                string newFilePath = mergeDir + "\\gifs\\" + file + append + ".gif";
                                if (!File.Exists(newFilePath))
                                {
                                    Console.WriteLine($"\tCopying {file}");
                                    File.Copy(filePath, newFilePath);
                                }
                                
                                // add to compare list for html generation
                                if (fileDiffers)
                                {
                                    if (!filesToCompare.Contains(mergeDir + "\\gifs\\" + file))
                                    //    Console.WriteLine($"FILES TO COMPARE ALREADY HAS {file}");
                                    //else
                                        filesToCompare.Add(mergeDir + "\\gifs\\" + file);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
        
            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);        

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                if (!File.Exists(tempPath))
                    file.CopyTo(tempPath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
    }
}