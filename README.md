# Links & More

A modern WPF application built with .NET 10 and WPF-UI, designed to help you organize and manage your links and more.

## Features

![Dashboard Preview](assets/dashboard.png)

- **Modern Dashboard**: A clean, organized view of all your saved content using Windows 11 Fluent Design with elegant diagonal gradient cards.
- **Content Types**:
  - 🔗 **Links**: Save URLs for quick access with one-click opening.
  - 📝 **Notes**: Store important text and thoughts.
  - 💻 **Snippets**: Keep code or text snippets ready to copy.
  - 🔐 **Passwords**: Securely store passwords with Windows Hello biometric authentication.
- **Smart Categorization**: Group your links and notes into custom categories (e.g., Work, Personal, Dev).
- **Instant Search**: Real-time filtering of all items by title or content as you type.

![Settings Preview](assets/settings.png)

### 🔐 Password Vault
- **Secure Encryption**: Passwords encrypted using Windows DPAPI (Data Protection API)
- **Windows Hello Integration**: Unlock passwords with fingerprint, face recognition, or PIN
- **Auto-Lock Security**: Passwords automatically re-lock after viewing
- **One-Click Copy**: Copy unlocked passwords to clipboard instantly

### ⚡ Quick Actions
  - One-click **Open** for links.
  - One-click **Copy to Clipboard** for notes and snippets.
  - Easy **Edit** and **Delete** functionality.
- **Customizable Themes**: Full support for Light and Dark modes, or sync with your Windows System theme.
- **Data Management**:
  - Local JSON storage (privacy-focused).
  - Easily find and manage your data file path via Settings.
- **Performance**: Optimized for speed with virtualization for large collections of items.

## Security

- **DPAPI Encryption**: All passwords are encrypted using Windows Data Protection API with user-scoped AES-256 equivalent encryption
- **No Cloud Storage**: All data stored locally on your machine for complete privacy
- **Biometric Authentication**: Leverages Windows Hello for secure, convenient password access
- **Atomic File Writes**: Data integrity protected with atomic file replacement to prevent corruption

## Technologies
- **Framework**: .NET 10.0 (Windows 10.0.19041.0 SDK)
- **UI Library**: WPF-UI (Windows 11 Fluent Design)
- **MVVM**: CommunityToolkit.Mvvm
- **Security**: Windows DPAPI, Windows Hello (UserConsentVerifier)

## Getting Started

### Prerequisites
- Windows 10 (build 19041) or later for Windows Hello support
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Visual Studio 2026 (recommended)

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/ian-cowley/LinksAndMore-.git
   ```
2. Open `LinksAndMore.sln` in Visual Studio.
3. Build and run the project.

## Development
To build the project from the command line:
```bash
dotnet build
```

## Credits
Developed by Ian Cowley and Antigravity (Google DeepMind).

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
