using UnityEngine;

namespace ImPulse.Enemies
{
    public class SymphonyMiniboss : BasicShooterEnemy
    {
        [Header("Miniboss Rhythm Settings")]
        public int cooldownMeasures = 1;

        private bool _isActiveMeasure = false;
        private int _cooldownCounter = 0;

        protected override void Awake()
        {
            base.Awake();
            _cooldownCounter = cooldownMeasures; 
        }

        protected override void OnBeat(int beatIndex)
        {
            if (_isDead) 
            {
                base.OnBeat(beatIndex);
                return;
            }

            // Calculate the measure logic BEFORE calling base.OnBeat so _isActiveMeasure is ready
            if (beatIndex % 4 == 0)
            {
                if (_isShooting) // inherited from BasicShooterEnemy
                {
                    if (_isActiveMeasure)
                    {
                        if (cooldownMeasures > 0)
                        {
                            _isActiveMeasure = false;
                            _cooldownCounter = 0;
                        }
                        else
                        {
                            _comboIndex = 0;
                        }
                    }
                    else
                    {
                        _cooldownCounter++;
                        
                        if (_cooldownCounter >= cooldownMeasures)
                        {
                            _isActiveMeasure = true;
                            _comboIndex = 0; 
                        }
                    }
                }
                else
                {
                    _isActiveMeasure = false;
                    _cooldownCounter = cooldownMeasures;
                }
            }

            // Let the base handle the death check and calling ProcessShootPattern
            base.OnBeat(beatIndex);
        }

        protected override void ProcessShootPattern(int step)
        {
            // Block the shot if we are resting this measure
            if (!_isActiveMeasure) return;

            // Otherwise, let BasicShooterEnemy fire normally
            base.ProcessShootPattern(step);
        }
    }
}