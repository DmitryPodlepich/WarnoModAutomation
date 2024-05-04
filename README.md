# WarnoModAutomation

This software is designed to automate the modification process of the "WARNO" game.

## How it works

1. Set up all settings related to paths of the installed game and specify a new mod name.
2. Click the "Create mod" button to create a game mod.
3. On the "Settings" tab, you can find a whole list of all settings you can change in the game. Be aware that some settings will only be applied to unit ammunition found in an AmmoFireRange.json file.
4. Use the "Modify" button to start the modification process. You can switch to full log to see all changes in the log text area.
5. After modification, you can use the "Generate" button to run the generation script, which will add the mod to the game.
6. You should now see your mod in the mods list in the game.

## Rollback

When a mod has been created, a local git repository will also be created. 
An initial commit will also be performed. 
You can rollback and monitor all game files changes with the git system after the modification and generation processes. 
Additionally, you can delete your mod with the "Delete" button and recreate it anytime you want.