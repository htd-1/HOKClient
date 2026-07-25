using Cysharp.Threading.Tasks;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    public abstract class ViewUnit:MonoBehaviour
    {
        //Pos
        public bool IsSyncPos;
        public bool PredictPos;
        public int PredictMaxCount;
        public bool SmoothPos;
        public float ViewPosAccer;
        
        //Dir
        public bool IsSyncDir;
        public bool SmoothDir;
        public float AngleMultiplier;
        public float ViewDirAccer;
        
        
        public Transform RoationRoot;
        
        
        protected AudioSource AudioSource;
        
        private int _predictCount;
        protected Vector3 ViewTargetPos;
        protected Vector3 ViewTargetDir;
        private LogicUnit _logicUnit;

        
        public virtual void Init(LogicUnit logicUnit)
        {
            _logicUnit = logicUnit;
            gameObject.name=logicUnit.UnitName+"_"+gameObject.name;

            transform.position = logicUnit.LogicPos.ConvertViewVector3();
            if(RoationRoot==null)RoationRoot=transform;
            RoationRoot.rotation=CalcRotation(logicUnit.LogicDir.ConvertViewVector3());
            AudioSource=GetComponent<AudioSource>();
        }

        protected virtual void Update()
        {
            if (IsSyncDir)
            {
                UpdateDirection();
            }

            if (IsSyncPos)
            {
                UpdatePosition();
            }
        }

        private void UpdateDirection()
        {
            if (_logicUnit.IsDirChanged)
            {
                ViewTargetDir=GetUnitViewDir();
                _logicUnit.IsDirChanged=false;
            }

            if (SmoothDir)
            {
                float threshold=GameTime.deltaTime*ViewDirAccer;
                float angle = Vector3.Angle(RoationRoot.forward, ViewTargetDir);
                float angleMult = (angle / 180f) * AngleMultiplier*GameTime.deltaTime;

                if (ViewTargetDir != Vector3.zero)
                {
                    Vector3 interDir = Vector3.Lerp(RoationRoot.forward, ViewTargetDir, threshold+angleMult);
                    RoationRoot.rotation = CalcRotation(interDir);
                }
                
            }
            else
            {
                RoationRoot.rotation=CalcRotation(ViewTargetDir);
            }
        }

        private void UpdatePosition()
        {
            if (PredictPos)
            {
                if (_logicUnit.IsPosChanged)
                {
                    ViewTargetPos=_logicUnit.LogicPos.ConvertViewVector3();
                    _logicUnit.IsPosChanged=false;
                    _predictCount = 0;
                }
                else
                {
                    if (_predictCount > PredictMaxCount) return;
                    float delta = GameTime.deltaTime;
                    //预测位置=逻辑速度*逻辑方向
                    var predicPos=delta*_logicUnit.LogicMoveSpeed.RawFloat*
                                  _logicUnit.LogicDir.ConvertViewVector3();
                    //新目标位置=表现目标位置+预测位置
                    ViewTargetPos+=predicPos;
                    ++_predictCount;
                }
                //实现平滑移动
                if (SmoothPos)
                {
                    transform.position=Vector3.Lerp(transform.position,ViewTargetPos,
                        ViewPosAccer*GameTime.deltaTime);
                }
                else
                {
                    transform.position = ViewTargetPos;
                    
                }

            }
            else
            {
               ForcePosSync();
            }

        }

        public void ForcePosSync()
        {
            transform.position = _logicUnit.LogicPos.ConvertViewVector3();
        }

        protected virtual Vector3 GetUnitViewDir()
        {
            return _logicUnit.LogicDir.ConvertViewVector3();
        }
        protected Quaternion CalcRotation(Vector3 targetDir)
        {
            return Quaternion.FromToRotation(Vector3.forward,targetDir);
        }

        public virtual void PlayAni(string aniName)
        {
            
        }

        public virtual void PlayAudio(string audioName, bool loop = false, int delay = 0)
        {
            // 转发壳：音频管线归 AudioSvc（实体音用本组件 AudioSource，保留多实体并发 + 3D 定位）。
            if (AudioSource == null) AudioSource = GetComponent<AudioSource>();
            AudioSvc.Instance.PlayEntityAudio(audioName, AudioSource, loop, delay).Forget();
        }
        
    }
}