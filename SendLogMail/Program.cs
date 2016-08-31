using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace SendLogMail
{
    class Program
    {
        static string fileDirOutput = "";
        static string fileDirBackup = "";
        static string writeFileName = "";
        static string strLog = "";

        static void Main(string[] args)
        {
            strLog = strLog + "\r\n" + DateTime.Now.ToString("HH:mm:ss") + " >> " + "=================================== Start of Execution ======================================";
            //-------------------------------------- Config File Location including Path
            string appPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string fileLocation = appPath + "\\config.dat";
            //-------------------------------------- Log File Location including Path
            string fileDirInput = "";
            string readFileName = "";
            //----------------------------------------------------------------------------

            string line;
            //string fileDirOutput = "";
            //string fileDirBackup = "";
            double timeDiff = 0;
            DateTime readDate = DateTime.Today;


            StreamReader configFile = getLogFile(fileLocation);

            if (configFile != null)
            {
                while ((line = configFile.ReadLine()) != null)
                {
                    string[] val = line.Split('=');
                    if (val[0].Trim() == "InputFiledir(Enter valid directory)")
                    { fileDirInput = val[1]; }

                    if (val[0].Trim() == "OutputFiledir(Enter valid directory)")
                    { fileDirOutput = val[1]; }

                    if (val[0].Trim() == "BackupDir(Enter valid directory)")
                    { fileDirBackup = val[1]; }

                    if (val[0].Trim() == "DateDiff(insert the number of days with sign)")
                    { timeDiff = double.Parse(val[1]); }
                }
                configFile.Close();
                //--------------------- write logs
                strLog = strLog + "\r\n" + DateTime.Now.ToString("HH:mm:ss") + " >> " + "Config File Read Complete";
                strLog = strLog + "\r\n" + DateTime.Now.ToString("HH:mm:ss") + " >> " + "Input File Location: " + fileDirInput;
                strLog = strLog + "\r\n" + DateTime.Now.ToString("HH:mm:ss") + " >> " + "Output File Location: " + fileDirOutput;
                strLog = strLog + "\r\n" + DateTime.Now.ToString("HH:mm:ss") + " >> " + "Backup File Location: " + fileDirBackup;
                //--------------------- Check if Folder exists
                CreateIfMissing(fileDirOutput);
                if(fileDirBackup == "")
                    fileDirBackup = appPath + "\\ExecutionFailLog\\";
                CreateIfMissing(fileDirBackup);
                //-----------------------------------------------------------------------------

                readDate = readDate.AddDays(timeDiff);

                readFileName = readDate.Year + "" + readDate.Month.ToString("00") + "" + readDate.Day.ToString("00");        //20160201.log
                fileDirInput = fileDirInput + "\\" + readFileName + ".log";

                //generate Write FileName
                writeFileName = "Tk" + readFileName + ".txt";
                readLogFile(fileDirInput, fileDirOutput, fileDirBackup);



            }
            else
            {
                // Config file not found
                fileDirBackup = appPath + "\\ExecutionFailLog\\";
                CreateIfMissing(fileDirBackup);
            }

            strLog = strLog + "\r\n" + DateTime.Now.ToString("HH:mm:ss") + " >> " + "=================================== End of Execution ======================================";

            writeLogFile(strLog);
        }

        static StreamReader getLogFile(string fileLocation)
        {
            strLog = strLog + "\r\n" + DateTime.Now.ToString("HH:mm:ss") + " >> " + "Reading File from: " + fileLocation;
            StreamReader file = null;

            //fileLocation = "D:\\temp\\config.dat";

            try
            {
                file = new StreamReader(fileLocation);
            }
            catch (Exception e)
            {
                strLog = strLog + "\r\n" + DateTime.Now.ToString("HH:mm:ss") + " >> " + "File Read Failed:";
                strLog = strLog + "\r\n" + DateTime.Now.ToString("HH:mm:ss") + " >> " + e.Message;
            }

            return file;
        }

        static void readLogFile(string logFileLocation, string txtFileSaveLocation, string backupLocation)
        {
            List<string> codeLst = new List<string>();
            List<string> nameLst = new List<string>();
            List<DateTime> enterLst = new List<DateTime>();
            List<DateTime> exitLst = new List<DateTime>();

            string line;
            int lineCount = 0;
            //Get Log File
            StreamReader TimeLogFile = getLogFile(logFileLocation);

            if (TimeLogFile != null)
            {
                while ((line = TimeLogFile.ReadLine()) != null)
                {
                    string[] strLine;
                    string[] strItems;

                    strLine = line.Split(new string[] { "values(" }, StringSplitOptions.None);

                    if (strLine.Length > 1)
                    {
                        strItems = strLine[1].Split(new string[] { "','" }, StringSplitOptions.None);

                        for (int i = 0; i < strItems.Length; i++)
                        {
                            strItems[i] = strItems[i].Replace("'", "");
                        }

                        if (strItems[4].Length > 4)
                        {
                            //string empCode = strItems[4].Substring(4, 6);
                            string empCode = strItems[4].ToString();
                            if (codeLst.Contains(empCode))
                            {
                                int index = codeLst.Select((item, i) => new { Item = item, Index = i }).First(x => x.Item == empCode).Index;

                                if (exitLst[index].CompareTo(DateTime.Parse(strItems[1])) < 0)
                                {
                                    exitLst[index] = DateTime.Parse(strItems[1]);
                                }
                            }
                            else
                            {
                                lineCount++;
                                codeLst.Add(empCode);
                                nameLst.Add(strItems[7]);
                                enterLst.Add(DateTime.Parse(strItems[1]));
                                exitLst.Add(DateTime.Parse(strItems[1]));
                            }
                        }
                    }
                }
                TimeLogFile.Close();
                strLog = strLog + "\r\n" + DateTime.Now.ToString("HH:mm:ss") + " >> " + "Input File Read Complete.";
                //-----------------------------------------------------------------------------
                writeTxtFile(codeLst, nameLst, enterLst, exitLst);


            }

        }

        static void writeTxtFile(List<string> codeLst, List<string> nameLst, List<DateTime> enterLst, List<DateTime> exitLst)
        {
            strLog = strLog + "\r\n" + DateTime.Now.ToString("HH:mm:ss") + " >> " + "Writing Output File at: " + fileDirOutput + "\\" + writeFileName;

            int cntr = 0;
            using (StreamWriter outputFile = new StreamWriter(fileDirOutput + "\\" + writeFileName))
            {
                foreach (string item in codeLst)
                {
                    string line = item.ToString() + "," + nameLst[cntr].ToString() + "," + enterLst[cntr].ToString("HH:mm:ss") + "," + exitLst[cntr].ToString("HH:mm:ss");
                    outputFile.WriteLine(line);
                    cntr++;
                }
            }
            strLog = strLog + "\r\n" + DateTime.Now.ToString("HH:mm:ss") + " >> " + "Output File Write Complete. " + cntr + " Line has been written.";
        }

        static void writeLogFile(string msg)
        {
            string fileName = fileDirBackup + "\\Log_" + DateTime.Today.ToString("yyyyMMdd") + writeFileName;

            using (FileStream fs = new FileStream(fileName, FileMode.Append, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine(msg);
            }
        }
        
        static void CreateIfMissing(string path)
        {
            bool folderExists = Directory.Exists(path);
            if (!folderExists)
                Directory.CreateDirectory(path);
        }
    }
}
