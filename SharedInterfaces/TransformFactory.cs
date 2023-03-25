using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace SharedInterfaces
{
    public class TransformFactory
    {
        private static TransformFactory sm_transformFactoryInstance = null;

        private static Dictionary<string, KeyValuePair<Type, Assembly>> m_transforms = new Dictionary<string, KeyValuePair<Type, Assembly>>();

        private static List<string> m_transformNames = new List<string>();

        private TransformFactory()
        {
        }

        public void LoadTransformTypes(string transformPath)
        {
            IEnumerable<string> fileList = Directory.EnumerateFiles(transformPath, "*.dll", SearchOption.AllDirectories);

            foreach (string fileName in fileList)
            {
                try
                {
                    // Load each assembly and register it
                    Assembly transform = Assembly.LoadFile(fileName);

                    try
                    {
                        Type[] assemblyTypes = transform.GetTypes();

                        foreach (Type registeredType in assemblyTypes)
                        {
                            if (registeredType.GetInterface("ITransform") != null)
                            {
                                // This one is a keeper
                                m_transforms.Add(registeredType.Name, new KeyValuePair<Type, Assembly>(registeredType, transform));
                                m_transformNames.Add(registeredType.Name);
                            }
                        }
                    }
                    catch (ArgumentException /*e*/)
                    {
                    }
                    catch (MethodAccessException /*e*/)
                    {
                    }
                    catch (MemberAccessException /*e*/)
                    {
                    }
                    catch (TargetInvocationException /*e*/)
                    {
                    }
                    catch (ReflectionTypeLoadException /*e*/)
                    {
                    }
                    catch (TypeLoadException /*e*/)
                    {
                    }
                    catch (NotSupportedException /*e*/)
                    {
                    }
                }

                // things to catch upon an assembly load
                catch (BadImageFormatException ex)
                {
                    ErrorMessage = ex.Message;
                }
                catch (FileLoadException ex)
                {
                    ErrorMessage = ex.Message;
                }
                catch (FileNotFoundException ex)
                {
                    ErrorMessage = ex.Message;
                }
            }
        }

        public void GetTransformNames(ref List<string> transformNames)
        {
            transformNames.Clear();

            foreach (string name in m_transformNames)
            {
                transformNames.Add(name);
            }
        }

        public string ErrorMessage
        {
            get;
            private set;
        }

        static public TransformFactory GetInstance()
        {
            if (sm_transformFactoryInstance == null)
            {
                sm_transformFactoryInstance = new TransformFactory();
            }

            return sm_transformFactoryInstance;
        }

        public ITransform GetTransform(string transformName)
        {
            ITransform transform = null;
            KeyValuePair<Type, Assembly> assembly;

            bool found = false;

            found = m_transforms.TryGetValue(transformName, out assembly);
            if (found == false)
            {
                // They may have passed in the full name
                string[] names = transformName.Split('.');

                found = m_transforms.TryGetValue(names[names.Length - 1], out assembly);
            }

            if (found == true)
            {

                Type transformType = assembly.Key;
                transform = (ITransform)assembly.Value.CreateInstance(transformType.FullName);
            }

            return transform;
        }

        public ITransform GetTransform(Type transformType)
        {
            ITransform transform = null;

            foreach (KeyValuePair<string, KeyValuePair<Type, Assembly>> transformData in m_transforms)
            {
                KeyValuePair<Type, Assembly> childKeyValue = transformData.Value;

                if (childKeyValue.Key == transformType)
                {
                    transform = (ITransform)childKeyValue.Value.CreateInstance(transformType.FullName);
                    if (transform != null)
                    {
                        break;
                    }
                }
            }

            return transform;
        }
    }
}
