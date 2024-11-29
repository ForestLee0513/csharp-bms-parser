using System;
using System.Collections.Generic;
namespace BMSParser
{
    public class Measure
    {
        public float Scale { get; set; } = 1.0f; // 4/4가 기본값이므로 1.0f로 지정

        // BGM //
        public SortedDictionary<double, List<NormalNote>> bgm = new SortedDictionary<double, List<NormalNote>>();
        public SortedDictionary<double, List<NormalNote>> Bgm { get { return bgm; } }

        // Note //
        private readonly Dictionary<int, SortedDictionary<double, NormalNote>> p1Lane = new Dictionary<int, SortedDictionary<double, NormalNote>>();
        public Dictionary<int, SortedDictionary<double, NormalNote>> P1Lane { get { return p1Lane; } }

        private readonly Dictionary<int, SortedDictionary<double, NormalNote>> p2Lane = new Dictionary<int, SortedDictionary<double, NormalNote>>();
        public Dictionary<int, SortedDictionary<double, NormalNote>> P2Lane { get { return p2Lane; } }

        // Long Note //
        private readonly Dictionary<int, SortedDictionary<double, LongNote>> p1LongLane = new Dictionary<int, SortedDictionary<double, LongNote>>();
        public Dictionary<int, SortedDictionary<double, LongNote>> P1LongLane { get { return p1LongLane; } }

        private readonly Dictionary<int, SortedDictionary<double, LongNote>> p2LongLane = new Dictionary<int, SortedDictionary<double, LongNote>>();
        public Dictionary<int, SortedDictionary<double, LongNote>> P2LongLane { get { return p2LongLane; } }

        // Hidden Note //
        private readonly Dictionary<int, SortedDictionary<double, HiddenNote>> p1HiddenLane = new Dictionary<int, SortedDictionary<double, HiddenNote>>();
        public Dictionary<int, SortedDictionary<double, HiddenNote>> P1HiddenLane { get { return p1HiddenLane; } }

        private readonly Dictionary<int, SortedDictionary<double, HiddenNote>> p2HiddenLane = new Dictionary<int, SortedDictionary<double, HiddenNote>>();
        public Dictionary<int, SortedDictionary<double, HiddenNote>> P2HiddenLane { get { return p2HiddenLane; } }

        // Mine Note //
        private readonly Dictionary<int, SortedDictionary<double, MineNote>> p1MineLane = new Dictionary<int, SortedDictionary<double, MineNote>>();
        public Dictionary<int, SortedDictionary<double, MineNote>> P1MineLane { get { return p1MineLane; } }

        private readonly Dictionary<int, SortedDictionary<double, MineNote>> p2MineLane = new Dictionary<int, SortedDictionary<double, MineNote>>();
        public Dictionary<int, SortedDictionary<double, MineNote>> P2MineLane { get { return p2MineLane; } }


        // GIMMIK - BPM, STOP, SCROLL EVENTS //
        private readonly SortedDictionary<double, BPM> bpmEvents = new SortedDictionary<double, BPM>();
        public SortedDictionary<double, BPM> BpmEvents { get { return bpmEvents; } }

        private readonly SortedDictionary<double, Stop> stopEvents = new SortedDictionary<double, Stop>();
        public SortedDictionary<double, Stop> StopEvents { get { return stopEvents; } }
        private readonly SortedDictionary<double, Scroll> scrollevents = new SortedDictionary<double, Scroll>();
        public SortedDictionary<double, Scroll> ScrollEvents { get { return scrollevents; } }

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
        /// <param name="pos">비트</param>
        /// <param name="wav">재생할 키음</param>
        public void AddBGM(double pos, int wav)
        {
            if (!bgm.ContainsKey(pos))
            {
                bgm.Add(pos, new List<NormalNote>());
            }

            bgm[pos].Add(new NormalNote(wav));
        }

        /// <summary>
        /// 해당 비트에 BPM 변경 이벤트를 추가합니다.
        /// </summary>
        /// <param name="pos">비트</param>
        /// <param name="newBpm">변경하려고 하는 BPM</param>
        public void AddBPMEvent(double pos, double newBpm)
        {
            bpmEvents.Add(pos, new BPM(newBpm));
        }

        /// <summary>
        /// 해당 비트에 정지 기믹을 추가합니다.
        /// </summary>
        /// <param name="pos">비트</param>
        /// <param name="stopDuration">정지하고 싶은 시간</param>
        public void AddStopEvent(double pos, double stopDuration)
        {
            stopEvents.Add(pos, new Stop(stopDuration));
        }

        /// <summary>
        /// 해당 비트에 BGA 애니메이션을 추가합니다.
        /// </summary>
        /// <param name="pos">비트</param>
        /// <param name="bmp">BGA 시퀀스 키</param>
        /// <param name="bgaType">BGA의 종류를 정의</param>
        public void AddBGA(double pos, int bmp, Define.BMSObject.BGA bgaType)
        {
            switch(bgaType)
            {
                case Define.BMSObject.BGA.BASE:
                    bgaEvents.Add(pos, new BGA(bmp));
                    break;
                case Define.BMSObject.BGA.LAYER:
                    layerBGAEvents.Add(pos, new BGA(bmp));
                    break;
                case Define.BMSObject.BGA.POOR:
                    poorBga.Add(pos, new BGA(bmp));
                    break;
            }
        }
        
        public void AddScroll(double pos, double scrollValue)
        {
            scrollevents.Add(pos, new Scroll(scrollValue));
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
                        p1Lane.Add(lane, new SortedDictionary<double, NormalNote>());
                    break;
                case Define.BMSObject.PlayerSide.P2:
                    if (!p2Lane.ContainsKey(lane))
                        p2Lane.Add(lane, new SortedDictionary<double, NormalNote>());
                    break;
            }
        }

        /// <summary>
        /// 해당 라인에 롱노트 채널을 초기화합니다.
        /// </summary>
        /// <param name="playerSide">플레이 영역 위치(1p / 2p)</param>
        /// <param name="lane">할당하고 싶은 키</param>
        private void ExtendLongNoteLine(Define.BMSObject.PlayerSide playerSide, int lane)
        {
            switch (playerSide)
            {
                case Define.BMSObject.PlayerSide.P1:
                    if (!p1LongLane.ContainsKey(lane))
                        p1LongLane.Add(lane, new SortedDictionary<double, LongNote>());
                    break;
                case Define.BMSObject.PlayerSide.P2:
                    if (!p2LongLane.ContainsKey(lane))
                        p2LongLane.Add(lane, new SortedDictionary<double, LongNote>());
                    break;
            }
        }

        /// <summary>
        /// 해당 라인에 숨겨진 채널을 초기화합니다.
        /// </summary>
        /// <param name="playerSide">플레이 영역 위치(1p / 2p)</param>
        /// <param name="lane">할당하고 싶은 키</param>
        private void ExtendHiddenNoteLine(Define.BMSObject.PlayerSide playerSide, int lane)
        {
            switch (playerSide)
            {
                case Define.BMSObject.PlayerSide.P1:
                    if (!p1HiddenLane.ContainsKey(lane))
                        p1HiddenLane.Add(lane, new SortedDictionary<double, HiddenNote>());
                    break;
                case Define.BMSObject.PlayerSide.P2:
                    if (!p2HiddenLane.ContainsKey(lane))
                        p2HiddenLane.Add(lane, new SortedDictionary<double, HiddenNote>());
                    break;
            }
        }

        /// <summary>
        /// 해당 라인에 지뢰노트 채널을 초기화합니다.
        /// </summary>
        /// <param name="playerSide">플레이 영역 위치(1p / 2p)</param>
        /// <param name="lane">할당하고 싶은 키</param>
        private void ExtendMineNoteLine(Define.BMSObject.PlayerSide playerSide, int lane)
        {
            switch (playerSide)
            {
                case Define.BMSObject.PlayerSide.P1:
                    if (!p1MineLane.ContainsKey(lane))
                        p1MineLane.Add(lane, new SortedDictionary<double, MineNote>());
                    break;
                case Define.BMSObject.PlayerSide.P2:
                    if (!p2MineLane.ContainsKey(lane))
                        p2MineLane.Add(lane, new SortedDictionary<double, MineNote>());
                    break;
            }
        }

        /// <summary>
        /// 해당 비트에 노트를 추가합니다.
        /// </summary>
        /// <param name="line">할당하고 싶은 키</param>
        /// <param name="pos">비트</param>
        /// <param name="wav">키음 ID</param>
        /// <param name="playerSide">플레이 영역 위치(1p / 2p)</param>
        public void AddNotes(int line, double pos, int wav, Define.BMSObject.PlayerSide playerSide)
        {
            ExtendNoteLine(playerSide, line);

            switch (playerSide)
            {
                case Define.BMSObject.PlayerSide.P1:
                    p1Lane[line].Add(pos, new NormalNote(wav));
                    break;
                case Define.BMSObject.PlayerSide.P2:
                    p2Lane[line].Add(pos, new NormalNote(wav));
                    break;
            }
        }
        public void AddLongNotes(int line, double pos, int wav, Define.BMSObject.PlayerSide playerSide)
        {
            ExtendLongNoteLine(playerSide, line);

            switch (playerSide)
            {
                case Define.BMSObject.PlayerSide.P1:
                    p1LongLane[line].Add(pos, new LongNote(wav));;
                    break;
                case Define.BMSObject.PlayerSide.P2:
                    p2LongLane[line].Add(pos, new LongNote(wav)); ;
                    break;
            }
        }

        /// <summary>
        /// 해당 비트에 숨겨진 노트를 추가합니다.
        /// </summary>
        /// <param name="line">할당하고 싶은 키</param>
        /// <param name="pos">비트</param>
        /// <param name="wav">키음 ID</param>
        /// <param name="playerSide">플레이 영역 위치(1p / 2p)</param>
        public void AddHiddenNotes(int line, double pos, int wav, Define.BMSObject.PlayerSide playerSide)
        {
            ExtendHiddenNoteLine(playerSide, line);

            switch (playerSide)
            {
                case Define.BMSObject.PlayerSide.P1:
                    p1HiddenLane[line].Add(pos, new HiddenNote(wav));
                    break;
                case Define.BMSObject.PlayerSide.P2:
                    p2HiddenLane[line].Add(pos, new HiddenNote(wav));
                    break;
            }
        }

        /// <summary>
        /// 해당 비트에 지뢰 노트를 추가합니다.
        /// </summary>
        /// <param name="line">할당하고 싶은 키</param>
        /// <param name="pos">비트</param>
        /// <param name="wav">키음 ID</param>
        /// <param name="playerSide">플레이 영역 위치(1p / 2p)</param>
        public void AddMineNotes(int line, double pos, double damage, Define.BMSObject.PlayerSide playerSide)
        {
            ExtendMineNoteLine(playerSide, line);

            switch (playerSide)
            {
                case Define.BMSObject.PlayerSide.P1:
                    p1MineLane[line].Add(pos, new MineNote(damage));
                    break;
                case Define.BMSObject.PlayerSide.P2:
                    p2MineLane[line].Add(pos, new MineNote(damage));
                    break;
            }
        }
    }
}
