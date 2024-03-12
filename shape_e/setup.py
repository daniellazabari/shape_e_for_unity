# Set anaconda environment
import os
import subprocess

COMMAND = "conda" if os.name == "nt" else "source"
YES_FLAG = "echo y" if os.name == "nt" else "yes"
DIRECTORY = os.path.dirname(os.path.realpath(__file__))

def create_conda_env():
    '''
    Set anaconda environment for shape_e
    '''

    os.system("conda create --name shape_e -y")
    subprocess.run(f"{COMMAND} activate shape_e && conda install -y pip && {YES_FLAG} | pip install transformers huggingface_hub git+https://github.com/huggingface/diffusers accelerate torch", shell=True) # for windows, inste
    subprocess.run(f"{COMMAND} activate shape_e && {YES_FLAG} | pip install charset-normalizer chardet", shell=True)
    
    if os.name == "posix":
        subprocess.run(f"{COMMAND} activate shape_e && conda install pytorch::pytorch torchvision torchaudio -c pytorch -y", shell=True) 
    else:
        subprocess.run(f"{COMMAND} activate shape_e && conda install pytorch torchvision torchaudio pytorch-cuda=11.8 -c pytorch -c nvidia -y", shell=True)
    
    # Get environment path
    if os.name == "nt":
        reuslt = subprocess.run(f"{COMMAND} activate shape_e && echo %CONDA_PREFIX%", shell=True, stdout=subprocess.PIPE) 
        base_conda_environment = reuslt.stdout.decode("utf-8").strip()
        conda_environment = os.path.join(base_conda_environment, "envs", "shape_e")
        python_exe = os.path.join(conda_environment, "python.exe")
    else:
        reuslt = subprocess.run(f"{COMMAND} activate shape_e && echo $CONDA_PREFIX", shell=True, stdout=subprocess.PIPE) 
        conda_environment = reuslt.stdout.decode("utf-8").strip()
        python_exe = os.path.join(conda_environment, "bin", "python")
    
    if not os.path.exists(os.path.join(DIRECTORY, "TextFiles")):
        os.makedirs(os.path.join(DIRECTORY, "TextFiles"))
    
    with open(os.path.join(DIRECTORY, "TextFiles", "PythonExe.txt"), "w") as f:
        f.write(python_exe)

if __name__ == "__main__":
    create_conda_env()
    
    
    
    


