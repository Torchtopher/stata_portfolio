import http.server, socketserver

class GzipHandler(http.server.SimpleHTTPRequestHandler):
    def end_headers(self):
        if self.path.endswith(".gz"):
            self.send_header("Content-Encoding", "gzip")
        elif self.path.endswith(".br"):
            self.send_header("Content-Encoding", "br")
        super().end_headers()

PORT = 8000
with socketserver.TCPServer(("", PORT), GzipHandler) as httpd:
    print(f"Serving at port {PORT}")
    httpd.serve_forever()
