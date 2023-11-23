using System;

// parsed from TABG Client

// Token: 0x02000160 RID: 352
public enum ClientEventCode : byte
{
    // Token: 0x0400089D RID: 2205
    NoCodeSet,
    // Token: 0x0400089E RID: 2206
    Info,
    // Token: 0x0400089F RID: 2207
    PlayerUpdate,
    // Token: 0x040008A0 RID: 2208
    Login,
    // Token: 0x040008A1 RID: 2209
    PlayerLeft,
    // Token: 0x040008A2 RID: 2210
    WeaponPickUpAccepted,
    // Token: 0x040008A3 RID: 2211
    PlayerDamaged,
    // Token: 0x040008A4 RID: 2212
    PlayerRespawn,
    // Token: 0x040008A5 RID: 2213
    SpawnGun,
    // Token: 0x040008A6 RID: 2214
    Data,
    // Token: 0x040008A7 RID: 2215
    GameStateChanged,
    // Token: 0x040008A8 RID: 2216
    ServerRestart,
    // Token: 0x040008A9 RID: 2217
    AllWeapons,
    // Token: 0x040008AA RID: 2218
    SeatAccepted,
    // Token: 0x040008AB RID: 2219
    PingRecieved,
    // Token: 0x040008AC RID: 2220
    CarUpdate,
    // Token: 0x040008AD RID: 2221
    PlayerMarkerEvent,
    // Token: 0x040008AE RID: 2222
    CarDamage,
    // Token: 0x040008AF RID: 2223
    TeamDead,
    // Token: 0x040008B0 RID: 2224
    ItemDrop,
    // Token: 0x040008B1 RID: 2225
    RingUpdate,
    // Token: 0x040008B2 RID: 2226
    PlayerAirplaneDropped,
    // Token: 0x040008B3 RID: 2227
    PlaneUpdate,
    // Token: 0x040008B4 RID: 2228
    AllDrop,
    // Token: 0x040008B5 RID: 2229
    RoomInitRequestResponse,
    // Token: 0x040008B6 RID: 2230
    ServerShutDown,
    // Token: 0x040008B7 RID: 2231
    ChunkEntry,
    // Token: 0x040008B8 RID: 2232
    ChunkExit,
    // Token: 0x040008B9 RID: 2233
    ReviveState,
    // Token: 0x040008BA RID: 2234
    PlayerEnteredChunk,
    // Token: 0x040008BB RID: 2235
    PlayerLeftChunk,
    // Token: 0x040008BC RID: 2236
    ChatMessage,
    // Token: 0x040008BD RID: 2237
    ItemThrown,
    // Token: 0x040008BE RID: 2238
    WeaponChanged,
    // Token: 0x040008BF RID: 2239
    GearChange,
    // Token: 0x040008C0 RID: 2240
    ThrowChatMessage,
    // Token: 0x040008C1 RID: 2241
    PlayerHealed,
    // Token: 0x040008C2 RID: 2242
    PlayerDead,
    // Token: 0x040008C3 RID: 2243
    PlayerStateChanged,
    // Token: 0x040008C4 RID: 2244
    PlayerEffect,
    // Token: 0x040008C5 RID: 2245
    PlayerDeathLootDrop,
    // Token: 0x040008C6 RID: 2246
    PlayerFire,
    // Token: 0x040008C7 RID: 2247
    ACMessage,
    // Token: 0x040008C8 RID: 2248
    PlayerLand,
    // Token: 0x040008C9 RID: 2249
    KickPlayer,
    // Token: 0x040008CA RID: 2250
    SpectatorForce,
    // Token: 0x040008CB RID: 2251
    ACRequestedData,
    // Token: 0x040008CC RID: 2252
    KickPlayerMessage,
    // Token: 0x040008CD RID: 2253
    PlayerRegMessage,
    // Token: 0x040008CE RID: 2254
    LootRemovedFromMap,
    // Token: 0x040008CF RID: 2255
    SoulDrop,
    // Token: 0x040008D0 RID: 2256
    CatchPhrase,
    // Token: 0x040008D1 RID: 2257
    BlessingRecieved,
    // Token: 0x040008D2 RID: 2258
    CurseCleansed,
    // Token: 0x040008D3 RID: 2259
    TakeDamageEvent,
    // Token: 0x040008D4 RID: 2260
    PlayerHealthStateChanged,
    // Token: 0x040008D5 RID: 2261
    DropSpawned,
    // Token: 0x040008D6 RID: 2262
    DropOpened,
    // Token: 0x040008D7 RID: 2263
    SyncProjectileEvent,
    // Token: 0x040008D8 RID: 2264
    PlayerFireSyncReturn,
    // Token: 0x040008D9 RID: 2265
    LootDropped,
    // Token: 0x040008DA RID: 2266
    ScoresChanged,
    // Token: 0x040008DB RID: 2267
    BombPlanted,
    // Token: 0x040008DC RID: 2268
    GunPurchasedAccepted,
    // Token: 0x040008DD RID: 2269
    MoneyChanged,
    // Token: 0x040008DE RID: 2270
    BombStartDefuse,
    // Token: 0x040008DF RID: 2271
    BombStopDefuse,
    // Token: 0x040008E0 RID: 2272
    PlayerLootRecieved,
    // Token: 0x040008E1 RID: 2273
    GraveStoneSpawned,
    // Token: 0x040008E2 RID: 2274
    PlayerRespawnFromBoss,
    // Token: 0x040008E3 RID: 2275
    NetworkPlayerTransmittedPackage,
    // Token: 0x040008E4 RID: 2276
    LootPackGiven,
    // Token: 0x040008E5 RID: 2277
    RecievedSteamID,
    // Token: 0x040008E6 RID: 2278
    ServerBrowserHandshake = 100,
    // Token: 0x040008E7 RID: 2279
    SteamTicketAuthenticationResponse,
    // Token: 0x040008E8 RID: 2280
    AssignSteamID = 163,
    // Token: 0x040008E9 RID: 2281
    FlagMatchOver,
    // Token: 0x040008EA RID: 2282
    BossFightResult = 166,
    // Token: 0x040008EB RID: 2283
    PackedServerData,
    // Token: 0x040008EC RID: 2284
    RequestStopDefuse,
    // Token: 0x040008ED RID: 2285
    RequestDefuseBomb,
    // Token: 0x040008EE RID: 2286
    RequestPlantBomb,
    // Token: 0x040008EF RID: 2287
    RequestPurchaseGun,
    // Token: 0x040008F0 RID: 2288
    RequestSyncProjectileEvent,
    // Token: 0x040008F1 RID: 2289
    RequestClickInteract,
    // Token: 0x040008F2 RID: 2290
    RequestTakeDamageEvent,
    // Token: 0x040008F3 RID: 2291
    RequestCurseCleanse,
    // Token: 0x040008F4 RID: 2292
    RequestHealthState,
    // Token: 0x040008F5 RID: 2293
    RequestBlessing,
    // Token: 0x040008F6 RID: 2294
    SendCatchPhrase,
    // Token: 0x040008F7 RID: 2295
    RequestRespawnTeamMate,
    // Token: 0x040008F8 RID: 2296
    RingDeath,
    // Token: 0x040008F9 RID: 2297
    KickMessage,
    // Token: 0x040008FA RID: 2298
    ACCacheState,
    // Token: 0x040008FB RID: 2299
    SpectatorRequest = 184,
    // Token: 0x040008FC RID: 2300
    CarTemporaryUpdate = 189,
    // Token: 0x040008FD RID: 2301
    PassangerUpdate,
    // Token: 0x040008FE RID: 2302
    PlayerState = 193,
    // Token: 0x040008FF RID: 2303
    RequestHealing,
    // Token: 0x04000900 RID: 2304
    PhotonCloseRoom,
    // Token: 0x04000901 RID: 2305
    WeaponChange = 198,
    // Token: 0x04000902 RID: 2306
    RequestItemThrow,
    // Token: 0x04000903 RID: 2307
    RequestAirplaneDrop = 203,
    // Token: 0x04000904 RID: 2308
    RoomInit = 202,
    // Token: 0x04000905 RID: 2309
    RequestItemDrop = 206,
    // Token: 0x04000906 RID: 2310
    AdminCommand,
    // Token: 0x04000907 RID: 2311
    PlayerMarkerAdded,
    // Token: 0x04000908 RID: 2312
    TABGPing = 210,
    // Token: 0x04000909 RID: 2313
    RequestSeat,
    // Token: 0x0400090A RID: 2314
    DamageEvent = 214,
    // Token: 0x0400090B RID: 2315
    RequestWeaponPickUp,
    // Token: 0x0400090C RID: 2316
    RequestWorldState = 218,
    // Token: 0x0400090D RID: 2317
    LobbyStats = 224,
    // Token: 0x0400090E RID: 2318
    GameServerOffline,
    // Token: 0x0400090F RID: 2319
    AppStats,
    // Token: 0x04000910 RID: 2320
    QueueState = 228,
    // Token: 0x04000911 RID: 2321
    GameListUpdate,
    // Token: 0x04000912 RID: 2322
    GameList,
    // Token: 0x04000913 RID: 2323
    Ping = 248,
    // Token: 0x04000914 RID: 2324
    EventCacheSlicePurged,
    // Token: 0x04000915 RID: 2325
    CacheSliceChanged,
    // Token: 0x04000916 RID: 2326
    ErrorInfo,
    // Token: 0x04000917 RID: 2327
    Disconnect,
    // Token: 0x04000918 RID: 2328
    PropertiesChanged,
    // Token: 0x04000919 RID: 2329
    Leave,
    // Token: 0x0400091A RID: 2330
    Join
}
