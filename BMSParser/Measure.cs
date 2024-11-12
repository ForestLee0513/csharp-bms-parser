using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Authentication.ExtendedProtection;
namespace BMSParser
{
    public class Measure
    {
        public float Scale { get; set; } = 1.0f; // 4/4가 기본값이므로 1.0f로 지정

        // BGM //
        public SortedDictionary<double, List<NormalNote>> bgm = new SortedDictionary<double, List<NormalNote>>();
        public SortedDictionary<double, List<NormalNote>> Bgm { get { return bgm; } }

        // Note //
        private readonly Dictionary<int, SortedDictionary<double, Note>> p1Lane = new Dictionary<int, SortedDictionary<double, Note>>();
        public Dictionary<int, SortedDictionary<double, Note>> P1Lane { get { return p1Lane; } }

        private readonly Dictionary<int, SortedDictionary<double, Note>> p2Lane = new Dictionary<int, SortedDictionary<double, Note>>();
        public Dictionary<int, SortedDictionary<double, Note>> P2Lane { get { return p2Lane; } }

        // GIMMIK - BPM, STOP EVENTS //
        private readonly SortedDictionary<double, BPM> bpmEvents = new SortedDictionary<double, BPM>();
        public SortedDictionary<double, BPM> BpmEvents { get { return bpmEvents; } }

        private readonly SortedDictionary<double, Stop> stopEvents = new SortedDictionary<double, Stop>();
        public SortedDictionary<double, Stop> StopEvents { get { return stopEvents; } }

        // BGA EVENT //
        // 메인 BGA채널
        private readonly SortedDictionary<double, BGA> bgaEvents = new SortedDictionary<double, BGA>();
        public SortedDictionary<double, BGA> BGAEvents { get { return bgaEvents; } }
        // 메인 위에 올라가는 레이어 채널
        private readonly SortedDictionary<double, BGA> layerBGAEvents = new SortedDictionary<double, BGA>();
        public SortedDictionary<double, BGA> LayerBGAEvents { get { return layerBGAEvents; } }
        // 미스레이어
        private readonly SortedDictionary<double, BGA> poorBga = new SortedDictionary<double, BGA>();
        public SortedDictionary<double, BGA> PoorBGA { get { return poorBga; } }

        public float PrevBeat { get; set; }

        public void SetPrevBeat(float prevBeat)
        {
            PrevBeat = prevBeat;
        }

        /// <summary>
        /// 해당 마디의 비트를 지정합니다.
        /// </summary>
        /// <param name="scale">지정하고 싶은 비트의 수 (4/4 박자는 1.0)</param>
        public void SetScale(float scale) => Scale = scale;

        /// <summary>
        /// BGM 채널에 키음을 할당합니다.
        /// </summary>
        /// <param name="beat">비트</param>
        /// <param name="wav">재생할 키음</param>
        public void AddBGM(float beat, int wav)
        {
            if (!bgm.ContainsKey(beat))
            {
                bgm.Add(beat, new List<NormalNote>());
            }

            bgm[beat].Add(new NormalNote(wav));
        }

        /// <summary>
        /// 해당 비트에 BPM 변경 이벤트를 추가합니다.
        /// </summary>
        /// <param name="beat">비트</param>
        /// <param name="newBpm">변경하려고 하는 BPM</param>
        public void AddBPMEvent(float beat, double newBpm)
        {
            bpmEvents.Add(beat, new BPM(newBpm));
        }

        /// <summary>
        /// 해당 비트에 정지 기믹을 추가합니다.
        /// </summary>
        /// <param name="beat">비트</param>
        /// <param name="stopDuration">정지하고 싶은 시간</param>
        public void AddStopEvent(float beat, double stopDuration)
        {
            stopEvents.Add(beat, new Stop(stopDuration));
        }

        /// <summary>
        /// 해당 비트에 BGA 애니메이션을 추가합니다.
        /// </summary>
        /// <param name="beat">비트</param>
        /// <param name="bmp">BGA 시퀀스 키</param>
        /// <param name="bgaType">BGA의 종류를 정의</param>
        public void AddBGA(float beat, int bmp, Define.BMSObject.BGA bgaType)
        {
            switch(bgaType)
            {
                case Define.BMSObject.BGA.BASE:
                    bgaEvents.Add(beat, new BGA(bmp));
                    break;
                case Define.BMSObject.BGA.LAYER:
                    layerBGAEvents.Add(beat, new BGA(bmp));
                    break;
                case Define.BMSObject.BGA.POOR:
                    poorBga.Add(beat, new BGA(bmp));
                    break;
            }
        }

        /// <summary>
        /// 해당 라인에 노트 채널을 초기화합니다.
        /// </summary>
        /// <param name="playerSide">플레이 영역 위치(1p / 2p)</param>
        /// <param name="lane">할당하고 싶은 키</param>
        private void ExtendNoteLine(Define.BMSObject.PlayerSide playerSide, int lane)
        {
            switch (playerSide)
            {
                case Define.BMSObject.PlayerSide.P1:
                    if (!p1Lane.ContainsKey(lane))
                        p1Lane.Add(lane, new SortedDictionary<double, Note>());
                    break;
                case Define.BMSObject.PlayerSide.P2:
                    if (!p2Lane.ContainsKey(lane))
                        p2Lane.Add(lane, new SortedDictionary<double, Note>());
                    break;
            }
        }

        /// <summary>
        /// 해당 비트에 노트를 추가합니다.
        /// </summary>
        /// <param name="line">할당하고 싶은 키</param>
        /// <param name="beat">비트</param>
        /// <param name="wav">키음</param>
        /// <param name="playerSide">플레이 영역 위치(1p / 2p)</param>
        public void AddNotes(int line, float beat, int wav, int lnobj, Define.BMSObject.PlayerSide playerSide)
        {
            ExtendNoteLine(playerSide, line);

            switch (playerSide)
            {
                case Define.BMSObject.PlayerSide.P1:
                    //p1Lane[line].Add(beat, new Note(wav));
                    break;
                case Define.BMSObject.PlayerSide.P2:

                    break;
            }
        }
    }
}
