%Global vars

%lighting
lightPos = [0 0 -10];
lightColor = [1 1 1];
ambient = [.3 .3 .3];
emissiveMat = [0 0 0];
ambientMat = [.5 .5 .5];
diffuseMat = [.05 .05 .05];
specularMat = [.3 .3 .3];
S = 4;

cameraPos = [0 0 -8];
cameraLookAt = [0 0 0];
objectPos = [0 0 0];
objectOri = [0 0 pi 1];
fov = 90.0;
near = 1;
far = 100;
ratio = 1;

A = importdata('shuttle_breneman_whitfield.raw', ' ', 0);
size = 1;
axis([-size size -size size]);
axis square;

%translate to origin
projMatrix = [1 0 0 0;
    0 1 0 0;
    0 0 1 0;
    [0 0 0] - objectPos 1];

%X rotation
a = [0 0 1];
b = -cameraPos + cameraLookAt; b(2) = 0;
ang = -atan2(norm(cross(a,b)),dot(a,b));
xRot = [cos(ang) 0 -sin(ang) 0
    0 1 0 0;
    sin(ang) 0 cos(ang) 0;
    0 0 0 1];
projMatrix = projMatrix * xRot;

%Y rotation
a = [0 0 1];
newLook = [-cameraPos + cameraLookAt 1] * xRot;
b = newLook(1:3); b(1) = 0;
ang = -atan2(norm(cross(a,b)),dot(a,b));
yRot =[1 0 0 0;
    0 cos(ang) sin(ang) 0;
    0 -sin(ang) cos(ang) 0;
    0 0 0 1];
projMatrix = projMatrix * yRot;

%translate from origin
projectMatrix = projMatrix * [1 0 0 0;
    0 1 0 0;
    0 0 1 0;
    objectPos - [0 0 0] 1];

%translate
projMatrix = projMatrix * [1 0 0 0;
    0 1 0 0;
    0 0 1 0;
    0 0 pdist([cameraPos;cameraLookAt]) 1];

%Projections
projMatrix = projMatrix * [1/ratio * cot(fov/2) 0 0 0;
    0 cot(fov/2) 0 0;
    0 0 far / (far - near) 1;
    0 0 -(far * near / (far - near)) 0];

%View Matrix
%X
ang = objectOri(1);
viewMatrix = [cos(ang) sin(ang) 0 0;
    -sin(ang) cos(ang) 0 0;
    0 0 1 0;
    0 0 0 1];
%Y
ang = objectOri(2);
viewMatrix = viewMatrix * [cos(ang) 0 -sin(ang) 0;
    0 1 0 0;
    sin(ang) 0 cos(ang) 0;
    0 0 0 1];

%Z
ang = objectOri(3);
viewMatrix = viewMatrix * [1 0 0 0;
    0 cos(ang) sin(ang) 0;
    0 -sin(ang) cos(ang) 0;
    0 0 0 1];

for i = 1:length(A)
    %get the correct triangle
    points = [A(i, 1:3) 1; A(i, 4:6) 1; A(i, 7:9) 1];
    points = points * viewMatrix;
    
    %back-cull
    v1 = points(2, 1:3) - points(1, 1:3);
    v2 = points(3, 1:3) - points(1, 1:3);
    v1 = v1 / norm(v1);
    v2 = v2 / norm(v2);
    normal = cross(v1, v2);
    normal = normal / norm(normal);
    dotProd = dot(normal, cameraPos - mean(points(:, 1:3)));
    
    %do lighting----------------------------------------------
    %ambient
    lighting = ambient .* ambientMat;
    
    %diffuse
    maxVal = max(dot(points(1, 1:3) - lightPos, normal), 0);
    cDiff = maxVal * lightColor .* diffuseMat;
    lighting = lighting + cDiff;
    
    %specular
    v1 = mean(points(:, 1:3)) - lightPos;
    v2 = mean(points(:, 1:3)) - cameraPos;
    hNorm = (v1 + v2) ./ abs(v1 + v2);
    hNorm = hNorm / norm(hNorm);
    cSpec = max(dot(normal, hNorm), 0) ^ S * lightColor .* specularMat;
    lighting = lighting + cSpec;
    
    %emissive
    lighting = lighting + emissiveMat;
    
    %clamp light vals
    lighting(:) = min(lighting(:), 1);
    
    %projection
    points = points * projMatrix;
    
    %Divide by W
    for j = 1:length(points(:,1))
        points(j, 1:3) = points(j, 1:3) / points(j, 4);
    end
    
    %Remove points with Z <= 0
    if points(1, 3) <= 0 || points(2, 3) <= 0 || points(3, 3) <= 0
        continue;
    end
    
    if(dotProd < 0)
        patch(points(:,1), points(:,2), lighting, 'EdgeColor', 'none');
        %patch(points(:,1), points(:,2), lighting);
    end
end