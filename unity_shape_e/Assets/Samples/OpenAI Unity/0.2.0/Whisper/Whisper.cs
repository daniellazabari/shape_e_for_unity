using OpenAI;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Diagnostics;

namespace Samples.Whisper
{
    public class Whisper : MonoBehaviour
    {
        [SerializeField] private Button recordButton;
        [SerializeField] private Image progressBar;
        [SerializeField] private Text message;
        [SerializeField] private Dropdown dropdown;
        
        private readonly string fileName = "output.wav";
        private readonly int duration = 5;
        
        private AudioClip clip;
        private bool isRecording;
        private float time;
        private OpenAIApi openai = new OpenAIApi();
        static private string writeTxtPath;
        static private string pythonScriptPath;
        static private string pythonExePath;
        private string textForPrompt;

        private void Start()
        {
            GetRequiredPaths();

            #if UNITY_WEBGL && !UNITY_EDITOR
            dropdown.options.Add(new Dropdown.OptionData("Microphone not supported on WebGL"));
            #else
            foreach (var device in Microphone.devices)
            {
                dropdown.options.Add(new Dropdown.OptionData(device));
            }
            recordButton.onClick.AddListener(StartRecording);
            dropdown.onValueChanged.AddListener(ChangeMicrophone);
            
            var index = PlayerPrefs.GetInt("user-mic-device-index");
            dropdown.SetValueWithoutNotify(index);
            #endif
        }

        private void ChangeMicrophone(int index)
        {
            PlayerPrefs.SetInt("user-mic-device-index", index);
        }
        
        private void StartRecording()
        {
            isRecording = true;
            recordButton.enabled = false;

            var index = PlayerPrefs.GetInt("user-mic-device-index");
            
            #if !UNITY_WEBGL
            clip = Microphone.Start(dropdown.options[index].text, false, duration, 44100);
            #endif
        }

        private async void EndRecording()
        {
            message.text = "Transcripting...";
            
            #if !UNITY_WEBGL
            Microphone.End(null);
            #endif
            
            byte[] data = SaveWav.Save(fileName, clip);
            
            var req = new CreateAudioTranscriptionsRequest
            {
                FileData = new FileData() {Data = data, Name = "audio.wav"},
                // File = Application.persistentDataPath + "/" + fileName,
                Model = "whisper-1",
                Language = "en"
            };
            var res = await openai.CreateAudioTranscription(req);

            progressBar.fillAmount = 0;
            message.text = res.Text;
            recordButton.enabled = true;
            
            textForPrompt = res.Text;
            message.text += "\nIs this the correct transcript? If not, click the record button and try again.";
            await System.Threading.Tasks.Task.Delay(5000);
            // If the record button was not clicked, create the .txt file
            if (!isRecording)
            {
                message.text = "Generating a 3D model. This may take a few minutes...";
                await System.Threading.Tasks.Task.Delay(5000);
                createTxtFile();
            }
        }

        private void Update()
        {
            if (isRecording)
            {
                time += Time.deltaTime;
                progressBar.fillAmount = time / duration;
                
                if (time >= duration)
                {
                    time = 0;
                    isRecording = false;
                    EndRecording();
                }
            }
        }

        private void createTxtFile()
        {
    
            if (!Directory.Exists(writeTxtPath))
            {
                Directory.CreateDirectory(writeTxtPath);
            }

            if (textForPrompt != "")
            {
                string txtDocumentContent = textForPrompt;

                // Write file to local path - make sure to have permission to write to this path
                string localTxtDocument = Path.Combine(writeTxtPath, "GeneratedText.txt");
                File.WriteAllText(localTxtDocument, txtDocumentContent);

                // After the txt file is created properly, run the python script
                if (File.Exists(localTxtDocument) && File.ReadAllText(localTxtDocument) == textForPrompt)
                {
                    runPythonScript();
                }

                string donePath = Path.Combine(Application.dataPath, "done.txt");
                if (File.Exists(donePath))
                {
                    File.Delete(donePath);
                    message.text = "finished generating the model! you can find it in your Assets\\PlyFiles folder";
                    
                    System.Threading.Thread.Sleep(5000);
                }
            }

        }

        private void runPythonScript()
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = pythonExePath;
            start.Arguments = "-u " + pythonScriptPath;
            start.UseShellExecute = false;
            start.CreateNoWindow = true;
            start.RedirectStandardOutput = true;

            Process pythonProcess = new Process();

            pythonProcess.StartInfo = start;
            pythonProcess.Start();
            
            pythonProcess.OutputDataReceived += new DataReceivedEventHandler(Process_OutputDataReceived);
            pythonProcess.BeginOutputReadLine();

            pythonProcess.WaitForExit();
            pythonProcess.Close();

        }

        static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            // Print what's printed to the console from the python script
            UnityEngine.Debug.Log(e.Data);

        }

        private static void GetRequiredPaths()
        {
            // Get parent directory of the application
            string appDirectory = Directory.GetParent(Application.dataPath).FullName;
            string parentDirectory = Directory.GetParent(appDirectory).FullName;

            // Get the paths of the python code and TextFiles folder
            string pythonCodeDir = Path.Combine(parentDirectory, "shape_e");
            writeTxtPath = Path.Combine(pythonCodeDir, "TextFiles");
            pythonScriptPath = Path.Combine(pythonCodeDir, "text_to_3d.py");

            // Path to python exe
            pythonExePath = File.ReadAllText(Path.Combine(writeTxtPath, "PythonExe.txt"));
        }
    }
}
