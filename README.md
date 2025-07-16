# TSafe

![Release (latest by date)](https://img.shields.io/github/v/release/TavstalDev/TSafe?style=plastic-square)
![Workflow Status](https://img.shields.io/github/actions/workflow/status/TavstalDev/TSafe/release.yml?branch=stable&label=build&style=plastic-square)
![License](https://img.shields.io/github/license/TavstalDev/TSafe?style=plastic-square)
![Downloads](https://img.shields.io/github/downloads/TavstalDev/TSafe/total?style=plastic-square)
![Issues](https://img.shields.io/github/issues/TavstalDev/TSafe?style=plastic-square)

### What is this?
This is the source code of a .NETFramework library written in C#. This library is a plugin made for Unturned 3.24.x+ servers.

### Description
A RocketMod plugin for Unturned offering virtual enderchest-style storage, saved securely in an SQL database for persistent, portable access.

### Features
- Private storage
- SQL persistence
- Global access
- Customizable Sizes 
- Admin controls


### Commands
| - means <b>or</b></br>
[] - means <b>required</b></br>
<> - means <b>optional</b>

---

<details>
<summary>/safe open</summary>
<b>Description:</b> Opens the virtual storage.
<br>
<b>Permission(s):</b> tsafe.command.safe.open
</details>

<details>
<summary>/safe clear</summary> Clears the virtual storage.
<b>Description:</b>
<br>
<b>Permission(s):</b> tsafe.command.safe.clear
</details>

<details>
<summary>/safeadmin open [player]</summary>
<b>Description:</b> Opens a specific player's virtual storage.
<br>
<b>Permission(s):</b> tsafe.command.safeadmin.open
</details>

<details>
<summary>/safeadmin clear [player] <itemId></summary> Clears a specific player's storage or all of a specific item
<b>Description:</b>
<br>
<b>Permission(s):</b> tsafe.command.safeadmin.clear
</details>

<details>
<summary>/safeadmin clearall <itemId></summary> Clears every unit of a specific item from all virtual storage.
<b>Description:</b>
<br>
<b>Permission(s):</b> tsafe.command.safeadmin.clearall
</details>
