using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Utilities;
using Microsoft.Xna.Framework.Content.Pipeline.Tasks;

namespace XNAConverter
{
    public class XNBBuilder : BuildContent
    {
        public BuildEngine buildEngine;
        public bool BuildAudioAsSoundEffects;
        public bool BuildAudioAsSongs;

        /// <summary>
        /// Create a new XNBBuilder.  Will default to uncompressed content, compiled to the "Windows" platform, and the "Reach" profile.
        /// </summary>
        public XNBBuilder()
        {
            this.TargetPlatform = "Windows";
            this.TargetProfile = "Reach";
            this.CompressContent = false;
            BuildAudioAsSongs = false;
            BuildAudioAsSoundEffects = false;
            buildEngine = new BuildEngine();
        }

        /// <summary>
        /// Create a new XNBBuilder.  Will default to being compiled to the "Windows" platform, and the "Reach" profile.
        /// </summary>
        /// <param name="CompressContent">True for compressed content, false for uncompressed content</param>
        public XNBBuilder(bool CompressContent)
        {
            this.CompressContent = CompressContent;
            BuildAudioAsSongs = false;
            BuildAudioAsSoundEffects = false;
            buildEngine = new BuildEngine();
        }

        /// <summary>
        /// Create a new XNBBuilder with platform, profile, and compression left to the programmer.
        /// </summary>
        /// <param name="targetPlatform">The target platform: Windows, XBox360, or WindowsPhone</param>
        /// <param name="targetProfile">The target profile: Reach, or HiDef</param>
        /// <param name="CompressContent">True for compressed content, false for uncompressed content</param>
        public XNBBuilder(string targetPlatform, string targetProfile,  bool CompressContent)
        {
            this.TargetPlatform = targetPlatform;
            this.TargetProfile = targetProfile;
            this.CompressContent = CompressContent;
            BuildAudioAsSongs = false;
            BuildAudioAsSoundEffects = false;
            buildEngine = new BuildEngine();
        }

        /// <summary>
        /// Returns a list of the errors recorded while processing files.
        /// </summary>
        public List<string> GetErrors()
        {
            return buildEngine.GetErrors();
        }
        
        /// <summary>
        /// Package content to a specified directory
        /// </summary>
        /// <param name="fileNames">A list of file names to be processed.</param>
        /// <param name="outputDirectory">The location to output the processed files.</param>
        /// <param name="shouldLog">Whether or not the program should log the build.</param>
        /// <param name="rootDirectory">The root directory of the content to be built.</param>
        /// <param name="buildStatus">The final status of the build.</param>
        /// <returns>Returns a list of processed files with their full-paths for handling by the program.</returns>
        public string[] PackageContent(string[] fileNames, string outputDirectory, bool shouldLog, string rootDirectory, out bool buildStatus)
        {
            string[] processedFileNames = null;
            buildStatus = false;
            try
            {
                if (!shouldLog)
                {
                    buildEngine.log = false;
                }
                else
                {
                    buildEngine = new BuildEngine("logfile.txt");
                }

                this.OutputDirectory = outputDirectory;
                this.RootDirectory = rootDirectory;
                this.SourceAssets = new TaskItem[fileNames.Length];

                for (int i = 0; i < this.SourceAssets.Length; ++i)
                {
                    string fileType = "." + (fileNames[i].Split('.')).Last<string>();
                    Dictionary<string, object> metaData = new Dictionary<string, object>();
                    if (".bmp.dds.dib.hdr.jpg.pfm.png.ppm.tga".Contains(fileType))
                    {
                        metaData.Add("Importer", "TextureImporter");
                        metaData.Add("Processor", "TextureProcessor");
                    }
                    else if (".fbx".Contains(fileType))
                    {
                        metaData.Add("Importer", "FbxImporter");
                        metaData.Add("Processor", "ModelProcessor");
                    }
                    else if (".fx".Contains(fileType))
                    {
                        metaData.Add("Importer", "EffectImporter");
                        metaData.Add("Processor", "EffectProcessor");
                    }
                    else if (".spritefont".Contains(fileType))
                    {
                        metaData.Add("Importer", "FontDescriptionImporter");
                        metaData.Add("Processor", "FontDescriptionProcessor");
                    }
                    else if (".x".Contains(fileType))
                    {
                        metaData.Add("Importer", "XImporter");
                        metaData.Add("Processor", "ModelProcessor");
                    }
                    else if (".xml".Contains(fileType))
                    {
                        metaData.Add("Importer", "XmlImporter");
                        metaData.Add("Processor", "PassThroughProcessor");
                    }
                    else if (".mp3".Contains(fileType))
                    {
                        metaData.Add("Importer", "Mp3Importer");
                        if (BuildAudioAsSoundEffects)
                            metaData.Add("Processor", "SoundEffectProcessor");
                        else if (BuildAudioAsSongs)
                            metaData.Add("Processor", "SongProcessor");
                        else
                            metaData.Add("Processor", "SoundEffectProcessor");
                    }
                    else if (".wma".Contains(fileType))
                    {
                        metaData.Add("Importer", "WmaImporter");
                        if (BuildAudioAsSoundEffects)
                            metaData.Add("Processor", "SoundEffectProcessor");
                        else if (BuildAudioAsSongs)
                            metaData.Add("Processor", "SongProcessor");
                        else
                            metaData.Add("Processor", "SoundEffectProcessor");
                    }
                    else if (".wav".Contains(fileType))
                    {
                        metaData.Add("Importer", "WavImporter");
                        if (BuildAudioAsSoundEffects)
                            metaData.Add("Processor", "SoundEffectProcessor");
                        else if (BuildAudioAsSongs)
                            metaData.Add("Processor", "SongProcessor");
                        else
                            metaData.Add("Processor", "SoundEffectProcessor");
                    }
                    else if (".wmv".Contains(fileType))
                    {
                        metaData.Add("Importer", "WmvImporter");
                        metaData.Add("Processor", "VideoProcessor");
                    }
                    metaData.Add("Name", Path.GetFileNameWithoutExtension(fileNames[i]));

                    this.SourceAssets[i] = new TaskItem(fileNames[i], metaData);
                }

                buildEngine.Begin();

                string xnaInstallFolder = Environment.ExpandEnvironmentVariables("%XNAGSv4%") + @"References\Windows\x86\";

                this.PipelineAssemblies = new TaskItem[]
                {
                    new TaskItem(xnaInstallFolder + "Microsoft.Xna.Framework.dll"), 
                    new TaskItem(xnaInstallFolder + "Microsoft.Xna.Framework.Content.Pipeline.dll"), 
                    new TaskItem(xnaInstallFolder + "Microsoft.Xna.Framework.Content.Pipeline.AudioImporters.dll"), 
                    new TaskItem(xnaInstallFolder + "Microsoft.Xna.Framework.Content.Pipeline.EffectImporter.dll"), 
                    new TaskItem(xnaInstallFolder + "Microsoft.Xna.Framework.Content.Pipeline.FBXImporter.dll"), 
                    new TaskItem(xnaInstallFolder + "Microsoft.Xna.Framework.Content.Pipeline.TextureImporter.dll"), 
                    new TaskItem(xnaInstallFolder + "Microsoft.Xna.Framework.Content.Pipeline.VideoImporters.dll"), 
                    new TaskItem(xnaInstallFolder + "Microsoft.Xna.Framework.Content.Pipeline.XImporter.dll"), 
                };

                this.BuildEngine = buildEngine;
                this.IntermediateDirectory = Directory.GetCurrentDirectory();
                buildStatus = this.Execute();

                if (this.OutputContentFiles != null)
                {
                    processedFileNames = new string[this.OutputContentFiles.Length];
                    for (int i = 0; i < processedFileNames.Length; ++i)
                    {
                        processedFileNames[i] = this.OutputContentFiles[i].ToString();
                    }
                }
            }
            catch { }
            finally
            {
                //No matter what, we want to flush and close the logger in the BuildEngine.
                buildEngine.End();
            }

            //Returns a list of files with their full path, allowing a file to be converted and then moved into appropriate
            //locations based on where the programmer decides it should go.  Mainly useful for dynamic conversions in game.
            return processedFileNames;
        }
    }
}
