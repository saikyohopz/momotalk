namespace Arkko.MomoTalk.Foundation.Utils;

public class IdWorker {
    private const long StartTimestamp = 1480166465631L;

    private const int SequenceBit = 12;
    
    private const int MachineBit = 5;

    private const int DatacenterBit = 5;

    private const long MaxDatacenterNum = -1L ^ (-1L << DatacenterBit);

    private const long MaxMachineNum = -1L ^ (-1L << MachineBit);

    private const long MaxSequence = -1L ^ (-1L << SequenceBit);

    private const int MachineLeft = SequenceBit;

    private const int DatacenterLeft = SequenceBit + MachineBit;

    private const int TimestampLeft = DatacenterLeft + DatacenterBit;

    private readonly long _datacenterId = 1;

    private readonly long _machineId = 1;

    private long _sequence;

    private long _lastTimestamp = -1L;

    private readonly static Lazy<IdWorker> Lazy = new(() => new IdWorker());

    public static IdWorker Instance => Lazy.Value;

    private IdWorker() { }

    public IdWorker(long cid, long mid) {
        if (cid is > MaxDatacenterNum or < 0) {
            throw new Exception($"中心Id应在(0,{MaxDatacenterNum})之间");
        }

        if (mid is > MaxMachineNum or < 0) {
            throw new Exception($"机器Id应在(0,{MaxMachineNum})之间");
        }
        
        _datacenterId = cid;
        _machineId = mid;
    }

    public long NextId() {
        long currentTimestamp = GetNewTimestamp();
        if (currentTimestamp < _lastTimestamp) throw new Exception("时钟倒退，Id生成失败！");

        if (currentTimestamp == _lastTimestamp) {
            //相同毫秒内，序列号自增
            _sequence = (_sequence + 1) & MaxSequence;
            //同一毫秒的序列数已经达到最大
            if (_sequence == 0L) currentTimestamp = GetNextMillis();
        } else {
            //不同毫秒内，序列号置为0
            _sequence = 0L;
        }

        _lastTimestamp = currentTimestamp;

        return (currentTimestamp - StartTimestamp) << TimestampLeft //时间戳部分
            | _datacenterId << DatacenterLeft //数据中心部分
            | _machineId << MachineLeft //机器标识部分
            | _sequence; //序列号部分
    }

    private long GetNextMillis() {
        long mill = GetNewTimestamp();

        while (mill <= _lastTimestamp) {
            mill = GetNewTimestamp();
        }

        return mill;
    }

    private static long GetNewTimestamp() {
        return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
    }
}
