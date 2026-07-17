import Navbar from './components/Navbar/Navbar';
import Footer from './components/Footer/Footer';
import './App.css';

function App() {
  return (
    <div className="app-layout">
      <Navbar />

      <main className="app-body">
        <section className="hero-section">
          <h2>Welcome to the Medical Center</h2>
          <p>
            Please sign in to make an appointment with a doctor and check your personal page.
          </p>
        </section>

        {/* В будущем здесь будет <Routes> из react-router-dom */}
      </main>

      <Footer />
    </div>
  );
}

export default App;