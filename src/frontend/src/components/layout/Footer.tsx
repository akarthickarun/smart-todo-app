export default function Footer() {
  const currentYear = new Date().getFullYear();

  return (
    <footer className="border-t border-slate-200 bg-white">
      <div className="container mx-auto px-4 py-6">
        <p className="text-center text-sm text-slate-500">
          © {currentYear} Smart Todo App. All rights reserved.
        </p>
      </div>
    </footer>
  );
}
