import './GlowFormInput.css';

export default function GlowFormInput({ children, label, error, className = '' }) {
  return (
    <div className={`glow-form-input ${className}`}>
      {label && <label className="glow-form-label">{label}</label>}
      <div className="glow-form-input-shell">
        <div className="glow-form-input-content">
          {children}
        </div>
      </div>
      {error && <span className="glow-form-error">{error}</span>}
    </div>
  );
}
