import urllib.request, urllib.parse, http.cookiejar, re

base = 'http://127.0.0.1:5187'
client = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))
def get(path):
    return client.open(base + path).read().decode()
def post(path, data):
    page = get(path)
    data['__RequestVerificationToken'] = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', page)[1]
    return client.open(base + path, urllib.parse.urlencode(data).encode()).read().decode()
page = get('/Account/Login')
assert 'jquery.validate.min.js' in page and 'jquery.validate.unobtrusive.min.js' in page
post('/Account/Login', {'Email': 'admin@university.local', 'Password': 'Admin123!'})
page = get('/Admin/CreateCourse')
assert 'jquery.validate.unobtrusive.min.js' in page and 'data-val-range' in page
page = post('/Admin/CreateCourse', {'CourseCode':'', 'CourseName':'', 'CreditHours':'7', 'StartDate':'2026-09-13','EndDate':'2026-09-14'})
assert 'The CourseCode field is required.' in page and 'between 1 and 6' in page
page = post('/Admin/CreateCourse', {'CourseCode':'VALIDATION', 'CourseName':'Test', 'CreditHours':'3', 'StartDate':'2026-09-13','EndDate':'2026-09-12'})
assert 'End date must be after start date.' in page
page = post('/Admin/AssignTeacher', {'CourseId':'0', 'TeacherId':'-1'})
assert 'Choose a valid course.' in page and 'Choose a valid teacher.' in page
page = post('/Admin/AssignTeacher', {'CourseId':'2147483647', 'TeacherId':'2147483647'})
assert 'Choose an existing teacher.' in page
for path in ['/Admin/Dashboard', '/Courses/Index', '/Admin/Teachers', '/Admin/Students', '/Admin/Enrollments']:
    assert 'An error occurred while processing your request' not in get(path)
print('PASS: login, validation script markup, invalid course inputs, date ordering, invalid/missing assignment IDs, and five admin pages')
