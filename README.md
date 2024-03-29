# Whisper-API-unity
A client library of OpenAI [Whisper transcription and translation API](https://platform.openai.com/docs/api-reference/audio) for Unity.

See also [official document](https://platform.openai.com/docs/guides/speech-to-text).

## Features

- Transcription
  - Speech audio file to text in speeched language. 
- Translation
  - Speech audio file to text in English.

## How to import by Unity Package Manager

Add following dependencies to your `/Packages/manifest.json`.

```json
{
  "dependencies": {
    "com.mochineko.whisper-api": "https://github.com/mochi-neko/Whisper-API-unity.git?path=/Assets/Mochineko/WhisperAPI#1.0.1",
    "com.mochineko.relent": "https://github.com/mochi-neko/Relent.git?path=/Assets/Mochineko/Relent#0.2.0",
    "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
    ...
  }
}
```

## How to use

Please generate your API key on [OpenAI](https://platform.openai.com/account/api-keys).

See [sample codes](./Assets/Mochineko/WhisperAPI.Samples).

You can customize handling of retries to fit needs of your project by [Relent](https://github.com/mochi-neko/Relent),
e.g. [PolicyFactory](./Assets/Mochineko/WhisperAPI.Samples/PolicyFactory.cs).

## Changelog

See [CHANGELOG](./CHANGELOG.md).

## 3rd party notices

See [NOTICE](./NOTICE.md).

## License

Licensed under the [MIT](./LICENSE) License.
