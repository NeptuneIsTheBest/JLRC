# JLRC (JSON Lyric)
基于WPF的JLRC动态歌词制作与显示套件

## JLRC格式
本质为JSON文件，经过字符标准化及Brotli压缩处理而成。
格式如下：

- JLRC
  - Dictionary info:
    - String tilte
    - String album
    - String artist
    - Double offset
    - Dictionary extend:
      - String songmd5
      - String language
      - ...
  - Dictionary lyric:
    - List text:
      - Dictionary
        - Double starttime
        - Double endtime
        - String text
        - String annotation
      - Dictionary
      - ...
    - List translation:
      - string translation
      - string translation
      - ...
### info
存放歌曲信息
### lyric
存放歌词文本以及翻译文本

## 使用说明
### JLRCCreator
目前支持的特性有：

+ 歌词信息写入
+ 动态歌词录入

目前尚未支持的特性有：

- 不支持注释（日语假名注释等）录入
- 不支持翻译文本写入
- 不支持写入扩展信息（extend字典）
- 目前只支持mp3和flac格式的音频文件

### JLRCPlayer
目前支持的特性有：

+ 动态歌词播放
+ 注释（日语假名等注音）读取
+ 显示歌词翻译

目前尚未支持的特性有：

- 不支持显示歌手和专辑等歌曲信息
- 不支持读取扩展信息（extend字典）
