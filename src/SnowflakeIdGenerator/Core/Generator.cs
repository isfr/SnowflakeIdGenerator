using System;

using SnowflakeIdGenerator.Core.Contracts;
using SnowflakeIdGenerator.Constants;

namespace SnowflakeIdGenerator.Core
{
    public class Generator : IGenerator
    {
        private readonly int _nodeId;

        private readonly long _customEpoch;

        private long _lastTimestamp = -1L;

        private long _sequence = 0L;

        private readonly object syncLock = new object();

        private const int _nodeIdShift = IdConstants.IdBits.TOTAL_BITS 
            - IdConstants.IdBits.EPOCH_BITS 
            - IdConstants.IdBits.NODE_ID_BITS;

        private const int _timeStampLeftShift = IdConstants.IdBits.TOTAL_BITS 
            - IdConstants.IdBits.EPOCH_BITS;

        public int NodeId
        {
            get { return _nodeId; }
        }

        public Generator(int nodeId)
            :this(nodeId, IdConstants.CUSTOM_EPOCH)
        {
        }

        public Generator(int nodeId, long customEpoch)
        {
            if(nodeId < 0 || nodeId > IdConstants.Limits.MAX_NODE_ID)
                throw new ArgumentOutOfRangeException(nameof(nodeId));

            _nodeId = nodeId;
            _customEpoch = customEpoch;
        }
        public long GenerateNextId()
        {
            lock (syncLock)
            {
                return GetNextId();
            }
        }

        private long GetNextId()
        {
            long currentTimestamp = this.GetTimestamp();

            if (currentTimestamp < this._lastTimestamp)
            {
                throw new InvalidOperationException("Invalid System Clock!");
            }

            if (currentTimestamp == _lastTimestamp)
            {
                this._sequence = (this._sequence + 1) & IdConstants.Limits.MAX_SEQUENCE;
                if (this._sequence == 0)
                {
                    currentTimestamp = this.WaitNextMillis(currentTimestamp);
                }
            }
            else
            {
                _sequence = 0;
            }

            _lastTimestamp = currentTimestamp;

#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand
            return ((currentTimestamp - this._customEpoch) << _timeStampLeftShift)
                | (this._nodeId << _nodeIdShift)
#pragma warning restore CS0675 // Bitwise-or operator used on a sign-extended operand
                | this._sequence;
                
        }

        private long GetTimestamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        private long WaitNextMillis(long currentTimestamp)
        {
            while (currentTimestamp == _lastTimestamp)
            {
                currentTimestamp = GetTimestamp();
            }
            return currentTimestamp;
        }

    }
}
