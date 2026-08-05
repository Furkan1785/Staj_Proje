namespace SakaryaERP.Middleware;

// Tüm statik/JS varlıklar yerelden servis edildiği için (~/lib/...) script-src/style-src 'self'
// yeterli. 'unsafe-inline' script için gerekli çünkü DataTables ekranlarının çoğu @section
// Scripts içinde inline JS kullanıyor (nonce/hash tabanlı CSP'ye geçmek büyük bir view refactor'ü
// gerektirir) — yani bu header, DataTables grid'lerindeki XSS'i (asıl düzeltme: render.text()/
// escapeHtml()) tek başına engellemez, ama harici bir domain'den script/iframe yüklenmesini ve
// clickjacking'i (frame-ancestors) engelleyerek ek bir savunma katmanı sağlar.
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
        context.Response.Headers.Append("Content-Security-Policy",
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline'; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data:; " +
            "font-src 'self'; " +
            "object-src 'none'; " +
            "frame-ancestors 'none'; " +
            "base-uri 'self'; " +
            "form-action 'self'");

        await _next(context);
    }
}
