# CityPress 0.2.0-beta.12

- **长评论明确写进主提示词。** 有足够重大事件素材时，本次生成就要给出完整长评，目标700至1400字、包含分层小标题，不再指望玩家自己追加要求。长评最低验收提高至500字。
- **审校直接修稿。** 发送完整编号稿件，允许补稿、扩写、合并和删稿；缺长评先交给审校补写，修订后重新核实事实、组织版面。仍不足才进入有次数限制的独立评论补写。修订记录、缓存与Word归档保留。
- **修复书法报头资源加载。** 游戏拒绝原来的内嵌字体地址，现改为随包加载的独立TTF资源。报头仍为可自定义文字，支持城市名与任意自定义报纸名。

已通过离线核心回归与浏览器预览；打包时另核对游戏程序集与界面回调。未使用玩家付费API。游戏内字体实际效果和真实模型生成仍需重启后验收。

安装后重启游戏。新编辑规则需重新生成报纸，仅重新导出不会改写历史正文。原有设置、密钥、存档与本地报纸不变。

## English

The primary prompt now requests a complete long opinion immediately. Review can add, expand, merge and delete copy using the complete numbered manuscript before final acceptance, with factual rechecking and bounded specialist fallback. Accepted long opinions require at least 500 characters; the requested target remains 700–1400. The calligraphy font now loads as a mounted TTF resource instead of a Cohtml-rejected data URI, preserving editable custom mastheads. Restart and regenerate to apply editorial changes. Native game rendering and live provider output still need acceptance; no paid player API was used.
