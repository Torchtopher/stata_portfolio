#!/usr/bin/env python3
"""
Unity Case Conflict Resolver
Fixes case-sensitive filesystem issues with Unity materials and assets.
"""

import os
import glob
from collections import defaultdict
from pathlib import Path

def find_case_conflicts(directory):
    """Find files that have case-insensitive duplicates in the same directory."""
    conflicts = defaultdict(list)
    
    # Unity asset extensions that can have case conflicts (excluding .cs files)
    extensions = ["*.mat", "*.asset", "*.prefab", "*.fbx", "*.png", "*.jpg", "*.jpeg", 
                 "*.tga", "*.psd", "*.tiff", "*.gif", "*.bmp", "*.iff", "*.pict",
                 "*.unity", "*.js", "*.shader", "*.cginc", "*.hlsl",
                 "*.wav", "*.mp3", "*.ogg", "*.aiff", "*.aif", "*.mod", "*.it",
                 "*.s3m", "*.xm", "*.mp4", "*.mov", "*.mpg", "*.mpeg", "*.mp4",
                 "*.avi", "*.asf", "*.dv", "*.ogv", "*.vp8", "*.webm"]
    
    # Find all Unity asset files
    all_files = []
    for ext in extensions:
        files = glob.glob(os.path.join(directory, f"**/{ext}"), recursive=True)
        all_files.extend(files)
    
    # Group files by directory and basename (case-insensitive)
    dir_conflicts = defaultdict(lambda: defaultdict(list))
    
    for file_path in all_files:
        dir_path = os.path.dirname(file_path)
        basename = os.path.basename(file_path)
        lowercase_name = basename.lower()
        dir_conflicts[dir_path][lowercase_name].append(file_path)
    
    # Only keep conflicts where multiple files exist in the same directory
    final_conflicts = {}
    for dir_path, dir_files in dir_conflicts.items():
        for lowercase_name, file_paths in dir_files.items():
            if len(file_paths) > 1:
                # Create a unique key that includes directory info
                key = f"{dir_path}/{lowercase_name}"
                final_conflicts[key] = file_paths
    
    return final_conflicts

def resolve_conflicts(conflicts, keep_preference="lowercase"):
    """Resolve conflicts by keeping one version and removing others."""
    actions = []
    
    for conflict_key, file_paths in conflicts.items():
        # Extract just the filename for display
        display_name = os.path.basename(conflict_key)
        print(f"\nFound conflict for '{display_name}' in {os.path.dirname(file_paths[0])}:")
        for i, path in enumerate(file_paths):
            print(f"  {i+1}. {os.path.basename(path)}")
        
        # Determine which file to keep
        if keep_preference == "lowercase":
            # Keep the lowercase version if it exists
            keep_file = None
            for path in file_paths:
                if os.path.basename(path).islower():
                    keep_file = path
                    break
            # If no lowercase version, keep the first one
            if not keep_file:
                keep_file = file_paths[0]
        else:
            # Keep the first file found
            keep_file = file_paths[0]
        
        print(f"  → Keeping: {os.path.basename(keep_file)}")
        
        # Mark others for deletion
        for path in file_paths:
            if path != keep_file:
                actions.append(("delete", path))
                # Only delete corresponding .meta file if the original file has one
                meta_path = path + ".meta"
                if os.path.exists(meta_path):
                    actions.append(("delete", meta_path))
                print(f"  → Will delete: {os.path.basename(path)}")
                if os.path.exists(meta_path):
                    print(f"  → Will delete: {os.path.basename(path)}.meta")
    
    return actions

def execute_actions(actions, dry_run=True):
    """Execute the planned actions."""
    if dry_run:
        print(f"\n=== DRY RUN - Would perform {len(actions)} actions ===")
    else:
        print(f"\n=== EXECUTING {len(actions)} actions ===")
    
    for action, path in actions:
        if action == "delete":
            if dry_run:
                print(f"Would delete: {path}")
            else:
                try:
                    os.remove(path)
                    print(f"Deleted: {path}")
                except OSError as e:
                    print(f"Error deleting {path}: {e}")

def clear_unity_cache(project_path):
    """Remove Unity's Library folder to force cache rebuild."""
    library_path = os.path.join(project_path, "Library")
    if os.path.exists(library_path):
        import shutil
        print(f"\nRemoving Unity Library cache: {library_path}")
        shutil.rmtree(library_path)
        print("Unity cache cleared. Unity will rebuild on next open.")
    else:
        print("No Library folder found.")

def main():
    project_path = "/home/chris/ripped_flightgoggles/ExportedProject"
    assets_path = os.path.join(project_path, "Assets")
    
    if not os.path.exists(assets_path):
        print(f"Assets directory not found: {assets_path}")
        return
    
    print(f"Scanning for case conflicts in entire Assets directory: {assets_path}")
    conflicts = find_case_conflicts(assets_path)
    
    if not conflicts:
        print("No case conflicts found!")
        return
    
    print(f"\nFound {len(conflicts)} conflicts:")
    actions = resolve_conflicts(conflicts, keep_preference="lowercase")
    
    # Show dry run first
    execute_actions(actions, dry_run=True)
    
    # Ask for confirmation
    print("\nThis will permanently delete files!")
    response = input("Proceed? (yes/no): ").strip().lower()
    
    if response == "yes":
        execute_actions(actions, dry_run=False)
        
        # Ask about clearing Unity cache
        response = input("\nClear Unity Library cache? (recommended - yes/no): ").strip().lower()
        if response == "yes":
            clear_unity_cache(project_path)
        
        print("\nDone! You can now safely open the project in Unity.")
    else:
        print("Cancelled.")

if __name__ == "__main__":
    main()