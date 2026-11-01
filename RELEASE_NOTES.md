# CityPress 0.2.0-beta.10

## 简体中文

- **素材先整理。** 新增本地素材整理工具，按建筑类别归并，保留实际建设变化，放置意图不作建设结果。常规运营线路清单、零伤亡且无明确影响的小事故、未核实含义的税收分组和电水内部数值不进入模型。生态保留有含义的监测检出占比，不把内部强度冒充现实污染等级。本地完整记录保留。
- **正文更接近报纸。** 金额、人口统一整理万/亿与小数精度，质量检查拦截资产规格、内部字段和长整数罗列；核实口径仅供内部使用。新闻、评论、局部修订采用同一素材取舍要求。
- **多议题评论和连续加版。** 评论任务按事实组织主评论及多篇不同角度短评，可补充其他长评；存在多个独立重大领域时至少保留两篇有效评论。所有主题按容量加版，评论第二篇长稿不再被强塞到第一版。
- **要闻导读。** 用已接受专题稿的核心事实形成一句导读，最终排版后写入“详见第几版”；薄小消息只留在专题，不重复凑要闻。
- **书法报头与竖排修复。** UI 内置马善政毛笔字体，自定义报纸名也适用，字体按 OFL 1.1 随包授权。带数字、复合长标题使用横排，短中文标题可竖排；旧报纸的不适合竖排标题在 UI 与 Word 中自动回退。版名区域加宽。

安装后需重启游戏；新内容需重新生成报纸，仅导出不会重新编辑旧正文。缓存版本已升级，不复用旧版嘈杂成稿。未调用玩家付费 API；离线模拟服务验证不能替代实际模型与游戏内验收。

## English

Local editorial tools now select, normalize and group source material before generation. Routine fleet inventories, prefab specifications, low-impact incidents and unverified units are excluded from provider input while complete local journals remain intact. Finished-copy checks reject leaked raw fields. Multiple major domains require distinct accepted opinions; rich themes and opinions can continue across editions. Front-page guides reference final edition numbers. The UI masthead bundles the OFL Ma Shan Zheng font; unsuitable numeric or long vertical headlines fall back to horizontal layout, including older papers and Word exports.

Restart the game and regenerate the issue for the new editorial policy. Export alone does not rewrite an archived issue. No paid player API was used in validation; native game and actual provider acceptance remain pending.
