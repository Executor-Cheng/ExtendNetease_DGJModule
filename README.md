插件功能
---
- 不再请求lwl12的api，改为本地发送请求，减轻api的压力
- 允许用户添加空闲歌单
- 允许用户登录网易云账号。若账号拥有音乐包/会员，点歌品质最高可至320Kbps

由于C#没有RSA_NO_PADDING这种填充方法,插件借助了BouncyCastle进行加密

项目的接口及加密方法均参照 [Binaryify的NeteaseCloudMusicApi项目](https://github.com/Binaryify/NeteaseCloudMusicApi) 进行翻译
