import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import 'bootstrap/dist/css/bootstrap.min.css'
//import './assets/scss/style.scss'
import './variables.css'
import './reset.css'
import './typography.css'
import './card.css'
import './button.css'
import './input.css'
import './animations.css'
import './color-utilities.css'
import './index.css'
import App from './App.jsx'
import 'bootstrap/dist/js/bootstrap.bundle.min.js'

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <App />
  </StrictMode>,
)
