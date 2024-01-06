<p align="center">
    <img height="100" src="../media/images/icon.png" />
</p>

<div align="center">
    <h1>Lethal Autocomplete</h1>
    <h3>This mod adds to the game ability to quickly print terminal commands. Also it allows to look through commands history.</h3>
</div>

---

**Tab** - autocomplete (click again to print other results), use **Esc** to exit terminal

**Up Arrow** - previous command

**Down Arrow** - next command

Keybinds are editable in the plugin configuration file.

---

### Installation

1) Install BepInEx in game folder

2) Download this and dependency mods

3) Install mods in BepInEx plugins folder

### Autocomplete weights

By default plugin sets weights for all available commands to value 10. The higher the value, the higher the priority of
issuing the command. Release archive contains preset of weights for autocomplete, so you don't have to set all by yourself.

Commands with weight equal to 0 won't be suggested for autocomplete.

```json
{
  "Words": [
    {
      "Word":"Buy",
      "Weight":10
    },
    {
      "Word":"ProFlashlight",
      "Weight":10
    },
    {
      "Word":"Confirm",
      "Weight":10
    }
  ]
}
```

## Change Log

### [0.4.1]

- Make save path in config file optional and use user's local path by default

### [0.4.0]

- Added autocomplete for commands containing several words, such as: buy, route, info, view.
- Added count option autocomplete for buy command

### [0.3.0]

- Added weights to commands for more precise autocomplete which can be edited in config file
- Commands history saving
- Small fixes

### [0.2.1]

- Non critical errors handling

### [0.2.0]

- Added plugin config file with keys binding

### [0.1.0]

- Added autocomplete on Left Shift button
- Added commands history look through Arrow Up/Down buttons

### Dependencies

[BepInEx](https://thunderstore.io/c/lethal-company/p/BepInEx/BepInExPack/)

[TerminalApi](https://thunderstore.io/c/lethal-company/p/NotAtomicBomb/TerminalApi/)

[InputUtils](https://thunderstore.io/c/lethal-company/p/Rune580/LethalCompany_InputUtils/)

### Contact

Discord: @aenclave

[Mod discussion](https://discord.com/channels/1169792572382773318/1190188482581246062)

### Source

https://github.com/IlyaChichkov/LethalAutocompleteMod
