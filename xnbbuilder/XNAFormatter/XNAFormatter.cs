using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using XNAConverter;


namespace XNAFormatter
{
    public partial class XNAFormatter : Form
    {
        string[] fileNames;
        string[] displayFileNames;
        string outputDirectory;

        public XNAFormatter()
        {
            InitializeComponent();
            platformBox.SelectedIndex = 0;
            profileBox.SelectedIndex = 0;
            outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Content");
            outputTextBox.Text = outputDirectory;
            _toolTip.SetToolTip(audioBox, "选择了将声音文件转换成背景音乐（music），不勾选将声音文件转成音效文件（sound）");
        }

        private void convertButton_Click(object sender, EventArgs e)
        {
            convert();
        }

        private void convert()
        {
            if (platformBox.Text.Length > 0 && profileBox.Text.Length > 0 && fileNames != null && fileNames.Length > 0)
            {
                statusLabel.Text = "";
                bool shouldLog = logBox.Checked;
                bool buildStatus = false;

                XNBBuilder packager = new XNBBuilder(platformBox.Text, profileBox.Text, compressBox.Checked);
                packager.BuildAudioAsSongs = audioBox.Checked;
                packager.PackageContent(fileNames, outputDirectory, shouldLog, Path.GetDirectoryName(fileNames[0]), out buildStatus);

                //buildStatus will be true for a successful build, or else false for one that has failed.
                if (!buildStatus)
                {
                    //If the build failed, we'll display that fact here.  If they logged the conversion, they'll also see a note
                    //to check the log, which is located in the same directory as the .exe and .dll are.
                    statusLabel.Text = "失败" + ((shouldLog)?("\r\n  显示日志"):(""));
                }
                else
                {
                    //The build completed successfully, and we can end our conversion.
                    statusLabel.Text = "成功";
                }

                try
                {
                    //During the build, a file called "ContentPipeline.xml" is created and is what is used by the Content Pipeline to
                    //determine what files are to be built.  If we leave it alone, on the next build, all the previous build's files
                    //are erased, for some internal reason.
                    File.Delete("ContentPipeline.xml");
                }
                catch { /* We don't care if the file doesn't exist (although it would be odd) */ }
                                
                fileNames = null;
                displayFileNames = null;
                sourceFileTextBox.Text = "";
            }
            else if (sourceFileTextBox.Text.Length == 0)
            {
                statusLabel.Text = "没设置源文件";
            }
        }

        private void sourceFileBrowseButton_Click(object sender, EventArgs e)
        {
            statusLabel.Text = "";

            sourceFileTextBox.Text = "";
            OpenFileDialog openFileDialogue = new OpenFileDialog();
            openFileDialogue.Multiselect = true;
            openFileDialogue.Filter = "All Available Types|*.bmp;*.dds;*.dib;*.hdr;*.jpg;*.pfm;*.png;*.ppm;*.tga;*.fbx;*.fx;*.spritefont;*.x;*.xml;*.mp3;*.wav;*.wma;*.wmv|" + 
                                      "Graphics Files|*.bmp;*.dds;*.dib;*.hdr;*.jpg;*.pfm;*.png;*.ppm;*.tga|" +
                                      "Autodesk FBX Files|*.fbx|" + 
                                      "Effects Files|*.fx|" + 
                                      "SpriteFont Files|*.spritefont|" + 
                                      "X Files|*.x|" + 
                                      "XNA XML Files|*.xml|" + 
                                      "Audio Files|*.mp3;*.wav;*.wma|" +
                                      "Video Files|*.wmv";

            if (openFileDialogue.ShowDialog() == DialogResult.OK)
            {
                fileNames = openFileDialogue.FileNames;
                displayFileNames = new string[fileNames.Length];
                if (openFileDialogue.FileNames.Length > 30)
                {
                    //Things get slow if we put a bunch of filenames into the textbox.  So if we have a lot, we don't.
                    //This does not affect the files we pass to be converted.
                    sourceFileTextBox.Text = "多个文件";
                }
                else
                {
                    //Format the file names for review before conversion, unless there are a lot of files.
                    StringBuilder sourceFiles = new StringBuilder();
                    for (int i = 0; i < openFileDialogue.FileNames.Length; ++i)
                    {
                        displayFileNames[i] = Path.GetFileName(fileNames[i]);
                        sourceFiles.Append("\"" + displayFileNames[i] + "\"" + ((i == openFileDialogue.FileNames.Length - 1) ? ("") : (", ")));
                    }
                    sourceFileTextBox.Text = sourceFiles.ToString();
                }
            }
        }

        private void outputBrowseButton_Click(object sender, EventArgs e)
        {
            statusLabel.Text = "";

            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                outputDirectory = folderBrowser.SelectedPath;
                outputTextBox.Text = outputDirectory;
            }
        }
    }
}
