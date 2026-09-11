import { useState, useEffect, useRef } from 'react'

// Reusable custom dropdown. Pulled out of AddTenant.jsx so EditTenantForm
// (inside TenantsList.jsx) can use the exact same look/behavior for
// property and unit selection.
function CustomSelect({ options, value, onChange, placeholder, disabled }) {
    const [open, setOpen] = useState(false);
    const ref = useRef(null);

    const selected = options.find(o => String(o.value) === String(value));

    useEffect(() => {
        const handleClickOutside = (e) => {
            if (ref.current && !ref.current.contains(e.target)) setOpen(false);
        };
        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside);
    }, []);

    return (
        <div className={`custom-select${disabled ? ' custom-select--disabled' : ''}`} ref={ref}>
            <div
                className={`custom-select__trigger${open ? ' custom-select__trigger--open' : ''}`}
                onClick={() => { if (!disabled) setOpen(o => !o); }}
            >
                <span className={selected ? 'custom-select__value' : 'custom-select__placeholder'}>
                    {selected ? selected.label : placeholder}
                </span>
                <span className={`custom-select__arrow${open ? ' custom-select__arrow--open' : ''}`}>▾</span>
            </div>
            {open && (
                <div className="custom-select__dropdown">
                    {options.map(opt => (
                        <div
                            key={opt.value}
                            className={`custom-select__option${String(opt.value) === String(value) ? ' custom-select__option--selected' : ''}`}
                            onClick={() => { onChange(opt.value); setOpen(false); }}
                        >
                            {opt.label}
                        </div>
                    ))}
                    {options.length === 0 && (
                        <div className="custom-select__empty">No options available</div>
                    )}
                </div>
            )}
        </div>
    );
}

export default CustomSelect;