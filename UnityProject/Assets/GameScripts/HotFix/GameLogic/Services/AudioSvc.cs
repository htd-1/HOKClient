using Cysharp.Threading.Tasks;
using TEngine;
using UnityEngine;
using AudioType=TEngine.AudioType;
namespace GameLogic
{
    /// <summary>
    /// 音频服务：浅封装 <see cref="IAudioModule"/>，统一业务层音频入口（学原版 HOKClient AudioSvc）。
    /// <para>BGM / UI 音 / 流程音三类走框架池（对应 <see cref="AudioType"/>.Music / UISound / Sound，由 <see cref="AudioAgent"/> 自管 AudioSource）；</para>
    /// <para>实体音（<see cref="PlayEntityAudio"/>）用实体自带 <see cref="AudioSource"/> + <see cref="IResourceModule"/> 异步加载，保留多实体并发 + 3D 空间定位，不受框架 Sound 池并发上限与 fadeout 打断。</para>
    /// <para><see cref="ViewUnit"/>.PlayAudio 仅作转发壳，加载+播放管线归本服务（不再散落 <see cref="ViewUnit"/>）。无状态转发单例，首次 <c>AudioSvc.Instance</c> 访问懒创建。</para>
    /// </summary>
    public sealed class AudioSvc : Singleton<AudioSvc>
    {
        /// <summary>
        /// 播放背景音乐（默认循环）。对应 <see cref="AudioType.Music"/>。
        /// </summary>
        public void PlayBGM(string path, bool loop = true)
        {
            if (string.IsNullOrEmpty(path)) return;
            GameModule.Audio.Play(AudioType.Music, path, loop);
        }

        /// <summary>
        /// 播放 UI 音效（异步 fire-and-forget）。对应 <see cref="AudioType.UISound"/>。
        /// </summary>
        public void PlayUIAudio(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            GameModule.Audio.Play(AudioType.UISound, path, bAsync: true);
        }

        /// <summary>
        /// 播放实体 / 战斗音效。对应 <see cref="AudioType.Sound"/>。
        /// <para>学原版签名带 <paramref name="source"/>：TEngine 池化播放由 <see cref="AudioAgent"/> 自管 AudioSource，
        /// 此处 <paramref name="source"/> 当前仅作语义保留 / 预留 3D 空间定位，底层不直接使用。</para>
        /// </summary>
        /// <summary>
        /// 播放流程级 / 单次战斗音效（无实体归属，如开战欢迎音）。走框架 Sound 池（低频，AgentHelperCount 并发足够）。
        /// </summary>
        public void PlaySound(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            GameModule.Audio.Play(AudioType.Sound, path);
        }

        /// <summary>
        /// 播放实体 / 战斗音效。用实体自带的 <paramref name="source"/> 播放（保留原版多实体并发 + 3D 空间定位，
        /// 不受框架 Sound 池 4 并发限制与打断），clip 经 <see cref="IResourceModule"/> 异步加载（非 Resources.Load）。
        /// </summary>
        public async UniTaskVoid PlayEntityAudio(string name, AudioSource source, bool loop = false, int delay = 0)
        {
            if (string.IsNullOrEmpty(name) || source == null) return;

            if (delay > 0) await UniTask.Delay(delay);

            var handle = GameModule.Resource.LoadAssetAsyncHandle<AudioClip>(name);
            await handle.ToUniTask();

            // 异步等待期间实体可能已销毁。
            if (source == null)
            {
                handle.Dispose();
                return;
            }

            var clip = handle.AssetObject as AudioClip;
            if (clip == null)
            {
                handle.Dispose();
                return;
            }

            source.clip = clip;
            source.loop = loop;
            source.Play();
        }
    }
}
