# BeatChallenges
BeatChallenges is a Beat Saber mod coded to make the game significantly harder. 
This is done by adding random minigame challenges throughout the level for players to complete/endure all while trying to survive the level.

## Setup
The mod itself is basically plug and play, grab a copy of BeatChallenges.dll and put it into the games "Plugins" folder.
But for any of the dependencies listed below, you must make sure you have them installed into your folder too otherwise the mod will not work.
- [BSIPA ^4.3.6](https://github.com/nike4613/BeatSaber-IPA-Reloaded/releases/tag/4.3.6)
- [BeatSaberMarkupLanguage ^1.12.5](https://github.com/monkeymanboy/BeatSaberMarkupLanguage/releases/tag/v1.12.5)
- [BS Utils ^1.14.2](https://github.com/Kylemc1413/Beat-Saber-Utils)
- [DataPuller ^2.1.15](https://github.com/WentTheFox/BSDataPuller/releases/tag/v2.1.15)

## About the project
Working on this was extremely hard since I pretty much know little to nothing about Beat Saber modding, it took me very many google searches and debug logging to achieve this.
Any bugs and issues I'll be sure to have a look at. More challenges will be added in the future.

## What to expect
Every 5-10 or so seconds a new challenge will appear for the player to complete. Failing to complete some of the challenges given will result in a level instafail.
Other challenges are a test of endurance, meaning the player must play through the level with modifications active for a certain period of time.
New challenges will not occur during the final 15 seconds of a song. All challenges are randomized, and they can repeat themselves multiple times, if lucky, they can appear twice in a row.
Multiple different challenges will not appear at the exact same time, only one will be picked at random to complete.
You will not receive warning beforehand, you must prepare yourself for a challenge at any given time. Yes I am quite evil.

Also I know some of you love that pause button, but pausing will not work, once paused the timer pauses for the challenge too, it will then resume once unpaused.

## Challenge list

### SUDDEN DEATH!
When SUDDEN DEATH! appears prepare to hit every note for the next 12 seconds. One wrong cut or miss will instantly fail the level.

### SPEED UP!
The whole level will speed up by a random percentage between 5% and 20% for 10 seconds.

### GHOST NOTES!
All notes become transparent for 10 seconds, the arrows on the notes remain visible and do not disappear when coming towards the player like they would with the "ghost notes" modifier.

### SLOW DOWN!
The level becomes extremely slow for 3 seconds, then reverts back to original speed.

### COMBO RUSH!
Quickly work towards achieving a 50 note combo, you have 15 seconds to do so or you will instafail the level. not to mention you must hold your 50 note combo until the timer runs out to pass.
Obtaining a 50 note combo then losing it is allowed, and you can work towards getting it back, but the timer will not reset. 15 seconds is your fixed amount of time.
