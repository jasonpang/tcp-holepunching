using System;

namespace TcpHolePunching.Messages
{
    /* Credits to Eric Maupin (Tempest) */
    public abstract class MessageBase
    {
        public MessageType MessageType { get; set; }

        public MessageBase(MessageType messageType)
        {
            MessageType = messageType;
        }

        /// <summary>
        /// Writes the message payload with <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The writer to use for writing the payload.</param>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is <c>null</c>.</exception>
        public virtual void WritePayload(IValueWriter writer)
        {
            writer.WriteInt32((int) MessageType);
        }

        /// <summary>
        /// Reads the message payload with <paramref name="reader"/>.
        /// </summary>
        /// <param name="reader">The reader to use for reading the payload.</param>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> is <c>null</c>.</exception>
        public virtual void ReadPayload(IValueReader reader)
        {
            MessageType = (MessageType) reader.ReadInt32();
        }
    }

    public enum MessageType
    {
        Internal,
        RequestIntroducerRegistration,
        ResponseIntroducerRegistration,
        RequestIntroducerIntroduction,
        ResponseIntroducerIntroduction,
    }
}
