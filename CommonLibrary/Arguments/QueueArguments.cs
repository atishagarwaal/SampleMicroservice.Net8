using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Arguments
{
    public class QueueArguments : Dictionary<string, object>
    {
        /// <summary>
        /// How long a queue can be unused for before it is automatically deleted (milliseconds).
        /// </summary>
        public int X_expires
        {
            get => ContainsKey("x-expires") ? (int)this["x-expires"] : 0;
            set => this["x-expires"] = value;
        }

        /// <summary>
        /// How long a message published to a queue can live before it is discarded (milliseconds).
        /// </summary>
        public int X_message_ttl
        {
            get => ContainsKey("x-message-ttl") ? (int)this["x-message-ttl"] : 0;
            set => this["x-message-ttl"] = value;
        }

        /// <summary>
        /// Sets the queue overflow behaviour. This determines what happens to messages 
        /// when the maximum length of a queue is reached. Valid values are:
        /// "drop-head", "reject-publish", or "reject-publish-dlx".
        /// The quorum queue type only supports "drop-head" and "reject-publish".
        /// </summary>
        public string? X_overflow
        {
            get => ContainsKey("x-overflow") ? this["x-overflow"] as string : null;
            set => this["x-overflow"] = value;
        }

        /// <summary>
        /// If set, ensures only one consumer at a time consumes from the queue 
        /// and fails over to another registered consumer in case the active one is cancelled or dies.
        /// </summary>
        public bool X_single_active_consumer
        {
            get => ContainsKey("x-single-active-consumer") ? (bool)this["x-single-active-consumer"] : false;
            set => this["x-single-active-consumer"] = value;
        }

        /// <summary>
        /// Optional name of an exchange to which messages will be republished if they are rejected or expire.
        /// </summary>
        public string? X_dead_letter_exchange
        {
            get => ContainsKey("x-dead-letter-exchange") ? this["x-dead-letter-exchange"] as string : null;
            set => this["x-dead-letter-exchange"] = value;
        }

        /// <summary>
        /// Optional replacement routing key to use when a message is dead-lettered.
        /// If this is not set, the message's original routing key will be used.
        /// </summary>
        public string? X_dead_letter_routing_key
        {
            get => ContainsKey("x-dead-letter-routing-key") ? this["x-dead-letter-routing-key"] as string : null;
            set => this["x-dead-letter-routing-key"] = value;
        }

        /// <summary>
        /// How many (ready) messages a queue can contain before it starts to drop them from its head.
        /// </summary>
        public int X_max_length
        {
            get => ContainsKey("x-max-length") ? (int)this["x-max-length"] : 0;
            set => this["x-max-length"] = value;
        }

        /// <summary>
        /// Total body size for ready messages a queue can contain before it starts to drop them from its head.
        /// </summary>
        public int X_max_length_bytes
        {
            get => ContainsKey("x-max-length-bytes") ? (int)this["x-max-length-bytes"] : 0;
            set => this["x-max-length-bytes"] = value;
        }

        /// <summary>
        /// Sets the rule by which the queue leader is located when declared on a cluster of nodes.
        /// Valid values are "client-local" (default) and "balanced".
        /// </summary>
        public string? X_queue_leader_locator
        {
            get => ContainsKey("x-queue-leader-locator") ? this["x-queue-leader-locator"] as string : null;
            set => this["x-queue-leader-locator"] = value;
        }
    }

}
