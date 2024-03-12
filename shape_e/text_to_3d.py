import torch
from diffusers import ShapEPipeline
from diffusers.utils import export_to_ply
import os

DEVICE = torch.device("cuda" if torch.cuda.is_available() else "cpu")
PROMPT_TEXT = ""
DIRECTORY = os.path.dirname(os.path.realpath(__file__))
PARENT_DIR = os.path.dirname(os.path.dirname(os.path.realpath(__file__)))
PLY_UNITY_PATH = os.path.join(PARENT_DIR, "unity_shape_e", "Assets", "PlyFiles")
DONE_FILE_DIR = os.path.join(PARENT_DIR, "unity_shape_e", "Assets")

PIPE = ShapEPipeline.from_pretrained("openai/shap-e", torch_dtype=torch.float32, variant="fp16")
PIPE = PIPE.to(DEVICE)

def set_prompt_text():
    print("Setting prompt text")
    text_file_path = os.path.join(DIRECTORY, "TextFiles", "GeneratedText.txt")

    with open(text_file_path, "r", encoding="utf-8") as file:
        curr_prompt = file.read()
        global PROMPT_TEXT
        if PROMPT_TEXT != curr_prompt:
            PROMPT_TEXT = curr_prompt

def text_to_3d():

    guidance_scale = 15.0
    prompt = PROMPT_TEXT

    images = PIPE(
        prompt,
        guidance_scale=guidance_scale,
        num_inference_steps=64,
        frame_size=128,
        output_type="mesh"
    ).images

    # Best output: num_inference_steps=64, guidance_scale=15.0, frame_size=128, output_type="mesh"

    if not os.path.exists(PLY_UNITY_PATH):
        os.makedirs(PLY_UNITY_PATH)

    file_name = PROMPT_TEXT.replace(" ", "_")
    file_path = os.path.join(PLY_UNITY_PATH, f"{file_name}.ply")
    export_to_ply(images[0], file_path)

if __name__ == "__main__":
    set_prompt_text()
    text_to_3d()
    with open(os.path.join(DONE_FILE_DIR, "done.txt"), "w") as f:
        f.write("done")
    


