# TTV App AI Instructions

## Project Overview
TTVApp is a .NET Core console application for downloading stories from the TTV (Tàng thư viện) platform for offline reading. The app fetches story content and chapters from the TTV API and converts them into HTML format for offline reading.

## Core Components

### Authentication & API Communication
- `StoryController.cs` handles all API communication with TTV platform
- Authentication uses IMEI and token-based system
- Key endpoints:
  - `get_token` - Acquires authentication token
  - `get_list_story_author` - Fetches story metadata
  - `get_list_chapter` - Gets chapter list
  - `get_content_chapter` - Downloads chapter content

### Data Models
- `Story.cs` - Core domain models:
  - `StoryModel` - Main story container with metadata and chapters
  - `ChapterModel` - Individual chapter data
  - `AuthorModel` - Author information
- `StoryData.cs` - API request/response models

## Key Workflows

### Story Download Process
1. Initialize `StoryController` with user ID, token and story ID
2. Fetch story metadata using `GetStoryContent()`
3. Retrieve chapter list via `GetChapterList()`
4. Download chapter contents using `GetChapterContent()`
5. Convert to HTML with table of contents

### Command Line Usage
```powershell
TTV.exe <userId> <token> <storyId> <fileName> <startChapter> <endChapter>
```

## Project Conventions

### Error Handling
- API errors return null responses rather than throwing exceptions
- Network/serialization errors are caught but not logged
- Success/failure indicated by object presence

### Data Persistence
- Binary serialization used for caching story data (*.bin files)
- HTML output for final rendered content
- Incremental saves during chapter downloads

### Security
- Authentication tokens required for all API calls
- SHA256 hashing used for API request signing
- Hardcoded app identifiers (IMEI, version) for API access

## Development Tips
- Unit tests in `TTVTest` project focus on hash verification
- Use UTF-8 encoding with BOM for all file operations
- Check `README.md` for instructions on obtaining API parameters
- Use Epubor Ultimate for converting HTML output to other formats

## Reference Files
- `Program.cs` - Main workflow orchestration
- `StoryController.cs` - API client implementation
- `Story.cs` - Domain models
- `StoryData.cs` - DTOs for API communication
