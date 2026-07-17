import './Footer.css';

const Footer = () => {
  return (
    <footer className="footer">
      <p>&copy; {new Date().getFullYear()} No rights have been protected</p>
    </footer>
  );
};

export default Footer;