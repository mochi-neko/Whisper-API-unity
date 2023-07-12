# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.1] - 2023-07-12

### Fixed
- Fix dependencies in `package.json`.

## [1.0.0] - 2023-07-03

### Added
- Add logging by `com.unity.logging`.
- Add detailed error handling by `Relent`.

### Changed
- Renewal interface of transcription API using `Relent` and `UniTask`.
- Renewal interface of translation API using `Relent` and `UniTask`.
- Rename directory, namespace and assembly from `Whisper_API` to `WhisperAPI`.
- Change Unity version from 2021.3 to 2022.3.

### Fixed
- Fix to set optional request parameters. 

## [0.1.0] - 2023-03-08

### Added
- Implement Whisper transcription API bindings to C#.
- Implement sample component of transcription.
- Implement Whisper translation API bindings to C#.
- Implement sample component of translation.