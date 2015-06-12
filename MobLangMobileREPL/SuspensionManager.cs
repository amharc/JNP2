using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MobLangMobileREPL
{
    /// <summary>
    /// SuspensionManager captures global session state to simplify process lifetime management
    /// for an application.  Note that session state will be automatically cleared under a variety
    /// of conditions and should only be used to store information that would be convenient to
    /// carry across sessions, but that should be discarded when an application crashes or is
    /// upgraded.
    /// </summary>
    internal sealed class SuspensionManager
    {
        private static Dictionary<string, object> _sessionState = new Dictionary<string, object>();
        private static readonly List<Type> _knownTypes = new List<Type>();
        private const string SessionStateFilename = "_sessionState.xml";

        public static Dictionary<string, object> SessionState
        {
            get { return _sessionState; }
        }

        public static List<Type> KnownTypes
        {
            get { return _knownTypes; }
        }

        public static async Task SaveAsync()
        {
            try
            {
                MemoryStream sessionData = new MemoryStream();
                DataContractSerializer serializer = new DataContractSerializer(typeof(Dictionary<string, object>), _knownTypes);
                serializer.WriteObject(sessionData, _sessionState);

                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(SessionStateFilename, CreationCollisionOption.ReplaceExisting);
                using (Stream fileStream = await file.OpenStreamForWriteAsync())
                {
                    sessionData.Seek(0, SeekOrigin.Begin);
                    await sessionData.CopyToAsync(fileStream);
                }
            }
            catch (Exception e)
            {
                throw new SuspensionManagerException(e);
            }
        }

        public static async Task RestoreAsync(String sessionBaseKey = null)
        {
            _sessionState = new Dictionary<String, Object>();

            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(SessionStateFilename);
                using (IInputStream inStream = await file.OpenSequentialReadAsync())
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof (Dictionary<string, object>),
                        _knownTypes);
                    _sessionState = (Dictionary<string, object>) serializer.ReadObject(inStream.AsStreamForRead());
                }
            }
            catch (Exception e)
            {
                throw new SuspensionManagerException(e);
            }
        }
    }
    public class SuspensionManagerException : Exception
    {
        public SuspensionManagerException()
        {
        }

        public SuspensionManagerException(Exception e)
            : base("SuspensionManager failed", e)
        {

        }
    }
}
