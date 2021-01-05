using System;
using System.Reactive.Subjects;
using Avalonia.Animation;
using Avalonia.Markup.Parsers;
using Avalonia.Styling;
using Avalonia.Styling.Activators;

namespace Material.Styles.Additional.Animations {
    public class InstantAnimation : ControllableAnimationBase {
        public string SelectorIsInstant { get; set; }

        public override IDisposable Apply(Animatable control, IClock clock, IObservable<bool> match, Action onComplete = null) {
            var selector = new SelectorParser((s, s1) => control.GetType()).Parse(SelectorIsInstant);
            var selectorMatch = selector.Match((IStyleable) control);

            if (!selectorMatch.IsMatch) {
                return base.Apply(control, clock, match, onComplete);
            }

            var isInstantSelector = new ObservableStyleActivatorSink();
            selectorMatch.Activator?.Subscribe(isInstantSelector);

            var instantAnimation = new Animation {
                Delay = Animation.Delay,
                Duration = TimeSpan.FromMilliseconds(1),
                Easing = Animation.Easing,
                FillMode = Animation.FillMode,
                IterationCount = Animation.IterationCount,
                SpeedRatio = Animation.SpeedRatio,
                PlaybackDirection = Animation.PlaybackDirection
            };
            instantAnimation.Children.AddRange(Animation.Children);
            var instantAnimationObserver = new Subject<bool>();
            var standardAnimationObserver = new Subject<bool>();

            match.Subscribe(b => {
                if (b) {
                    if (isInstantSelector.LastResult) {
                        instantAnimationObserver.OnNext(true);
                    }
                    else {
                        standardAnimationObserver.OnNext(true);
                    }
                }
                else {
                    instantAnimationObserver.OnNext(false);
                    standardAnimationObserver.OnNext(false);
                }
            });

            instantAnimation.Apply(control, clock, instantAnimationObserver, onComplete);
            return base.Apply(control, clock, standardAnimationObserver, onComplete);
        }
    }

    public class ObservableStyleActivatorSink : IStyleActivatorSink {
        public ISubject<bool> Next => _next;
        private readonly Subject<bool> _next = new Subject<bool>();

        public bool LastResult { get; private set; }

        public void OnNext(bool value, int tag) {
            LastResult = value;
            _next.OnNext(value);
        }
    }
}