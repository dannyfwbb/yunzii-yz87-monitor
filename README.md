# Yunzii YZ87 Battery Monitor

Simple tray application to monitor the battery level of the Yunzii YZ87 keyboard.
**Currently works only when using 2.4Ghz dongle.**

## Features
- **Dynamic Tray Icon**: Displays battery level with color-coded indicators:
  - **Green**: Above 50%
  - **Orange**: 20% - 50%
  - **Red**: Below 20%
  - **Gray**: Error state
- **Windows Startup Option**: Enable/disable auto-start with Windows from the tray menu.

## Installation
1. Download the latest release from the [Releases](https://github.com/dannyfwbb/yunzii-yz87-monitor/releases).
2. Extract archive and run `YZ87Monitor.exe`. The app will appear in the system tray.

## Usage
- **Tray Icon**: Hover to view battery percentage.
- **Context Menu**:
  - **App Name & Version**: Displays the app name with the current version. Click to navigate to the GitHub repository.
  - **Start with Windows**: Toggle startup behavior.
  - **Exit**: Quit the application.

## Development
### Prerequisites
- Visual Studio 2022+
- .NET 9.0 SDK

### Build Instructions
1. Clone the repository:
   ```bash
   git clone https://github.com/dannyfwbb/yunzii-yz87-monitor.git
2. Open YZ87Monitor.sln in Visual Studio.
3. Build and run the solution.
