#
# This is an example VCL file for Varnish.
#
# It does not do anything by default, delegating control to the
# builtin VCL. The builtin VCL is called when there is no explicit
# return statement.
#
# See the VCL chapters in the Users Guide for a comprehensive documentation
# at https://www.varnish-cache.org/docs/.

# Marker to tell the VCL compiler that this VCL has been written with the
# 4.0 or 4.1 syntax.
vcl 4.1;

# Default backend definition. Set this to point to your content server.
backend default {
    .host = "127.0.0.1";
    .port = "5145";
}


sub vcl_backend_response {
# Uh, wtf?? When we have a 0s ttl (or none set), Varnish will NOT make conditional requests!
# So maybe, it would be better to ONLY set this to this minimal value if no s-maxage or maxage was sent, or is 0
# that way, we can still take advantage of the max-age the server sent us...
     set beresp.ttl = 0.000001s;
     #set beresp.grace = 2m;
     set beresp.keep = 10s; # keep for 10 seconds. The cache content will only be delivered if the backend fetch returned a 304!
}