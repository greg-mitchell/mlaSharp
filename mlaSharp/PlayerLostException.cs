using System;
using System.Runtime.Serialization;

namespace mlaSharp
{
	/// <summary>
	/// An exception that is thrown when a player loses "asynchronously" - ex. drawing when there are no cards left in library
	/// </summary>
	public class PlayerLostException : Exception, ISerializable
	{
		public Player losingPlayer;
		
		public PlayerLostException (string message)
			: base(message)
		{ }
		
		public PlayerLostException (string message, Exception innerException)
			: base(message, innerException)
		{ }
		
		public PlayerLostException (string message, Exception innerException, Player losingPlayer)
			: base(message,innerException)
		{
			this.losingPlayer = losingPlayer;
		}
		
		protected PlayerLostException(SerializationInfo info, StreamingContext context)
		{
			// TODO: add implementation	
		}
	}
}

