using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace SqlTrigger.Data
{
    public interface IListener<T> : IDisposable
    {
        void Start();
        void Stop();
        event EventHandler<T> OnMessageRecieved;
    }

    public class Listener : IListener<SqlEventArgs>
    {
        private readonly Broker _broker;
        private readonly string _queue;

        public Listener(Broker broker, string queue)
        {
            _broker = broker;
            _queue = queue;
        }

        public event EventHandler<SqlEventArgs> OnMessageRecieved;


        private bool _listening = false;
        public void Start()
        {
            _listening = true;
            new Thread(() =>
                {
                    while (_listening)
                    {
                        RecieveNextMessage();
                    }
                }).Start();
        }

        private void RecieveNextMessage()
        {
            string msgType, msg;
            Guid group, handle;
            _broker.Receive(_queue, out msgType, out msg, out group, out handle);

            if (msg != null && OnMessageRecieved != null)
                OnMessageRecieved(this, new SqlEventArgs(msgType, msg, group, handle));
        }

        public void Stop()
        {
            _listening = false;
        }

        public void Dispose()
        {
            _broker.Dispose();
        }
    }

    public class SqlEventArgs
    {
        public string MessageType { get; protected set; }
        public XDocument Message { get; protected set; }
        public Guid Group { get; protected set; }
        public Guid Handle { get; protected set; }

        public SqlEventArgs(string messageType, string message, Guid group, Guid handle)
        {
            MessageType = messageType;
            Message = XDocument.Parse(message);
            Group = @group;
            Handle = handle;
        }
    }


    public class Broker : IDisposable
    {
        SqlConnection con;
        SqlTransaction tran;
        SqlCommand cmd;

        public Broker(string connectionString)
        {
            con = new SqlConnection(connectionString);
        }

        private void EnsureOpen()
        {
            if (con.State == ConnectionState.Closed)
                con.Open();
        }

        public void BeginTransaction()
        {
            EnsureOpen();
            if (tran != null)
                throw (new InvalidOperationException("Broker already has open transaction"));
            tran = con.BeginTransaction();
        }

        public void Rollback()
        {
            if (tran == null)
                throw (new InvalidOperationException("Broker not operating in a transactional context"));
            tran.Rollback();
            tran = null;
        }

        public void Commit()
        {
            if (tran == null)
                throw (new InvalidOperationException("Broker not operating in a transactional context"));
            tran.Commit();
            tran = null;
        }

        public void Send(Guid dialogHandle, string msg, string msgType)
        {
            EnsureOpen();
            SqlCommand cmd = con.CreateCommand();
            cmd.Transaction = tran;

            SqlParameter paramDialogHandle = new SqlParameter("@dh", SqlDbType.UniqueIdentifier);
            paramDialogHandle.Value = dialogHandle;
            cmd.Parameters.Add(paramDialogHandle);

            SqlParameter paramMsg = new SqlParameter("@msg", SqlDbType.NVarChar, msg.Length);
            paramMsg.Value = msg;
            cmd.Parameters.Add(paramMsg);

            cmd.CommandText = "SEND ON CONVERSATION @dh MESSAGE TYPE [" + msgType + "] (@msg)";

            cmd.ExecuteNonQuery();
        }

        public void Receive(string queueName, out string msgType, out string msg, out Guid ConversationGroup, out Guid dialogHandle)
        {
            EnsureOpen();
            msgType = null;
            msg = null;
            ConversationGroup = Guid.Empty;
            dialogHandle = Guid.Empty;

            cmd = con.CreateCommand();
            cmd.Transaction = tran;

            SqlParameter paramMsgType = new SqlParameter("@msgtype", SqlDbType.NVarChar, 256);
            paramMsgType.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(paramMsgType);

            SqlParameter paramMsg = new SqlParameter("@msg", SqlDbType.NVarChar, 4000);
            paramMsg.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(paramMsg);

            SqlParameter paramConversationGroup = new SqlParameter("@cg", SqlDbType.UniqueIdentifier);
            paramConversationGroup.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(paramConversationGroup);

            SqlParameter paramDialogHandle = new SqlParameter("@dh", SqlDbType.UniqueIdentifier);
            paramDialogHandle.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(paramDialogHandle);

            cmd.CommandText = "WAITFOR (RECEIVE TOP(1)  @msgtype = message_type_name, " +
                              "@msg = message_body, " +
                              "@cg = conversation_group_id, " +
                              "@dh = conversation_handle " +
                              "FROM [" + queueName + "]) " +
                              ", TIMEOUT 5000";


            cmd.ExecuteNonQuery();

            if (!(paramMsgType.Value is DBNull))
            {
                msgType = (string)paramMsgType.Value;
                msg = (string)paramMsg.Value;
                ConversationGroup = (System.Guid)paramConversationGroup.Value;
                dialogHandle = (System.Guid)paramDialogHandle.Value;
            }
        }

        public void EndDialog(Guid dialogHandle)
        {
            EnsureOpen();
            SqlCommand cmd = con.CreateCommand();
            cmd.Transaction = tran;

            SqlParameter paramDialogHandle = new SqlParameter("@dh", SqlDbType.UniqueIdentifier);
            paramDialogHandle.Value = dialogHandle;
            cmd.Parameters.Add(paramDialogHandle);

            cmd.CommandText = "END CONVERSATION @dh ";

            cmd.ExecuteNonQuery();
        }

        private bool disposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    con.Dispose();
                }
                disposed = true;
            }
        }

    }
}