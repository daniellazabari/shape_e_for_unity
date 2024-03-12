import os

# Get parent directory
PARENT_DIR = os.path.dirname(os.path.dirname(os.path.realpath(__file__)))
PLY_UNITY_PATH = os.path.join(PARENT_DIR, "unity_shape_e", "Assets", "PlyFiles")
DONE_FILE_DIR = os.path.join(PARENT_DIR, "unity_shape_e", "Assets")